using System.Diagnostics;
using FastFood.MVC.Data;
using FastFood.MVC.Models;
using FastFood.MVC.Services;
using FastFood.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly MessageService _messageService;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            MessageService messageService)
        {
            _logger = logger;
            _context = context;
            _messageService = messageService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();

            var carousels = await _context.Products
                .Where(p => p.IsCarouselItem)
                .ToListAsync();

            var products = await _context.Products
                .Where(p => !p.IsCarouselItem)
                .ToListAsync();

            var top4PerCategory = products
                .GroupBy(p => p.CategoryID)
                .SelectMany(g => g
                    .OrderByDescending(p => p.ProductID)
                    .Take(4)
                )
                .ToList();

            var top4Promotion = await _context.Promotions
                .Where(p => p.StartDate <= DateTime.Now && p.ExpiryDate > DateTime.Now)
                .OrderByDescending(p => p.PromotionID)
                .Take(4)
                .ToListAsync();

            HomeViewModel model = new HomeViewModel(
                categories,
                carousels,
                top4PerCategory.AsEnumerable(),
                top4Promotion.AsEnumerable());
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            Message model = new Message();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Message model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _messageService.CreateMessageAsync(model);

            TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ với chúng tôi. Chúng tôi sẽ phản hồi trong thời gian sớm nhất!";
            return RedirectToAction(nameof(Contact));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
