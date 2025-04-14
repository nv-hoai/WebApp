﻿using FastFood.MVC.Data;
using FastFood.MVC.Models;
using FastFood.MVC.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.MVC.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.UserRoles
                .Include(x => x.Role)
                .Include(x => x.User)
                .Where(x => x.Role.Name != "Admin")
                .Select(x => new
                {
                    x.User.Id,
                    x.User.Email,
                    x.User.PhoneNumber,
                    x.Role.Name
                }).ToListAsync();


            var viewModel = users
                .Select((user, index) => new UserViewModel
                {
                    Index = index + 1,  // Start from 1 instead of 0
                    Id = user.Id,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber!,
                    RoleName = user.Name!
                })
                .ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, model.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, model.Password);

                user.PhoneNumber = model.PhoneNumber;

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, model.RoleName);

                    var customer = new Customer
                    {
                        UserID = user.Id,
                        User = user,
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return PartialView("_RegisterContent", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _context.UserRoles
                .Include(x => x.Role)
                .Include(x => x.User)
                .Select(x => new UserViewModel
                {
                    Id = x.UserId,
                    Email = x.User.Email!,
                    PhoneNumber = x.User.PhoneNumber!,
                    RoleName = x.Role.Name!

                })
                .FirstOrDefaultAsync(x => x.Id == id);

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                if (model.Email != user.Email)
                    await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
                if (model.PhoneNumber != user.PhoneNumber)
                    user.PhoneNumber = model.PhoneNumber;
                var result = await _userManager.UpdateAsync(user);
                
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(model.RoleName))
                {
                    var resultRemove = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    var resultAdd = await _userManager.AddToRoleAsync(user, model.RoleName);
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User updated their profile successfully.");
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User was deleted successfully.");
                    return RedirectToAction("Index");
                }
            }

            return View("Error");
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
