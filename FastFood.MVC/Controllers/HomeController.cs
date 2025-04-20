using System.Diagnostics;
using FastFood.MVC.Data;
using FastFood.MVC.Models;
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

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
