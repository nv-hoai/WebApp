using FastFood.MVC.Data;
using FastFood.MVC.Models;
using FastFood.MVC.Services;
using FastFood.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FastFood.MVC.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    [Route("Admin/Management")]
    public class AdminController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<AdminController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IDashboardService _dashboardService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AdminController> logger,
            IEmailSender emailSender,
            ApplicationDbContext context,
            IDashboardService dashboardService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _dashboardService = dashboardService;
        }
        [Route("Accounts")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.UserRoles
                .Include(x => x.Role)
                .Include(x => x.User)
                .Where(x => x.Role.Name != "Admin")
                .Select(x => new
                {
                    x.User.Email,
                    x.User.PhoneNumber,
                    x.Role.Name
                }).ToListAsync();


            var model = users
                .Select((user, index) => new UserViewModel
                {
                    Index = index + 1,  // Start from 1 instead of 0
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber!,
                    RoleName = user.Name!
                })
                .AsEnumerable();

            return View(model);
        }

        public async Task<IActionResult> Dashboard()
        {
            var viewModel = await _dashboardService.GetDashboardDataAsync();
            return View(viewModel);
        }

        [HttpPost]
        [Route("Accounts/Register")]
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

                    switch(model.RoleName)
                    {
                        case "Customer":
                            var customer = new Customer
                            {
                                UserID = user.Id,
                            };
                            _context.Customers.Add(customer);
                            break;
                        case "Employee":
                            var employee = new Employee
                            {
                                UserID = user.Id,
                            };
                            _context.Employees.Add(employee);
                            break;
                        case "Shipper":
                            var shipper = new Shipper
                            {
                                UserID = user.Id,
                            };
                            _context.Shippers.Add(shipper);
                            break;
                    }

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

            return PartialView("_RegisterModal", model);
        }

        [HttpGet]
        [Route("Accounts/Edit")]
        public async Task<IActionResult> Edit(string email)
        {
            var user = await _context.UserRoles
                .Include(x => x.Role)
                .Include(x => x.User)
                .Select(x => new UserViewModel
                {
                    Email = x.User.Email!,
                    PhoneNumber = x.User.PhoneNumber!,
                    RoleName = x.Role.Name!

                })
                .FirstOrDefaultAsync(x => x.Email == email);

            return View(user);
        }

        [HttpPost]
        [Route("Accounts/Edit")]
        public async Task<IActionResult> Edit(string oldEmail, UserViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(oldEmail);

            ModelState.Remove("Password");

            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (model.Email != oldEmail)
                {
                    await _userStore.SetUserNameAsync(user, model.Email, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
                }

                if (model.PhoneNumber != user.PhoneNumber)
                    user.PhoneNumber = model.PhoneNumber;

                var currentRoles = await _userManager.GetRolesAsync(user);

                if (!currentRoles.Contains(model.RoleName))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, model.RoleName);
                    var oldRole = currentRoles.FirstOrDefault();
                    switch(oldRole)
                    {
                        case "Customer":
                            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.UserID == user.Id);
                            if (customer != null) _context.Customers.Remove(customer);
                            break;
                        case "Employee":
                            var employee = await _context.Employees.FirstOrDefaultAsync(x => x.UserID == user.Id);
                            if (employee != null) _context.Employees.Remove(employee);
                            break;
                        case "Shipper":
                            var shipper = await _context.Shippers.FirstOrDefaultAsync(x => x.UserID == user.Id);
                            if (shipper != null) _context.Shippers.Remove(shipper);
                            break;
                    }
                    switch (model.RoleName)
                    {
                        case "Customer":
                            var customer = new Customer
                            {
                                UserID = user.Id,
                            };
                            _context.Customers.Add(customer);
                            break;
                        case "Employee":
                            var employee = new Employee
                            {
                                UserID = user.Id,
                            };
                            _context.Employees.Add(employee);
                            break;
                        case "Shipper":
                            var shipper = new Shipper
                            {
                                UserID = user.Id,
                            };
                            _context.Shippers.Add(shipper);
                            break;
                    }
                    await _context.SaveChangesAsync();
                }

                bool changePassword = true;

                if (!model.NewPassword.IsNullOrEmpty())
                {
                    changePassword = false;
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result1 = await _userManager.ResetPasswordAsync(user, token, model.NewPassword!);

                    if (result1.Succeeded)
                    {
                        changePassword = true;
                    }
                    else
                    {
                        foreach (var error in result1.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }

                var result2 = await _userManager.UpdateAsync(user);

                if (changePassword && result2.Succeeded)
                {
                    _logger.LogInformation("User updated their profile successfully.");
                    return RedirectToAction("Index");
                }
                else if (!result2.Succeeded)
                {
                    foreach (var error in result2.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        [Route("Accounts/Delete")]
        public async Task<IActionResult> Delete(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

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
