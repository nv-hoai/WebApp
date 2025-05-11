using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Data;
using FastFood.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using FastFood.MVC.Services;

namespace FastFood.MVC.Controllers
{
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public PromotionController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(string? promotion, string? discountPercentSort)
        {
            var query = _context.Promotions
                .AsQueryable();

            if (!string.IsNullOrEmpty(promotion))
            {
                query = query.Where(p => p.Name.Contains(promotion));
            }

            switch (discountPercentSort)
            {
                case "asc":
                    query = query.OrderBy(p => p.DiscountPercent);
                    break;
                case "desc":
                    query = query.OrderByDescending(p => p.DiscountPercent);
                    break;
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }

            var promotions = await query.ToListAsync();

            return View(promotions);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions
                .Include(p => p.Product)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.PromotionID == id);
            if (promotion == null)
            {
                return NotFound();
            }

            return View(promotion);
        }

        [HttpGet]
        [Authorize(Policy = "AdminAccess")]
        public IActionResult Create()
        {
            Promotion model = new Promotion();
            ViewData["Product"] = new SelectList(_context.Products, "ProductID", "Name");
            ViewData["Category"] = new SelectList(_context.Categories, "CategoryID", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Create(Promotion promotion)
        {
            if (ModelState.IsValid)
            {

                _context.Add(promotion);
                await _context.SaveChangesAsync();
                var promotionAdded = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.PromotionID == promotion.PromotionID);
                var customers = await _context.Customers.ToListAsync();
                foreach (var customer in customers)
                {
                    await _notificationService.CreateNotification(
                        customer.UserID,
                        $"Có khuyến mãi mới, xem ngay!",
                        $"/Promotion/Details/{promotionAdded.ProductID}",
                        "fa-check-circle");
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Product"] = new SelectList(_context.Products, "ProductID", "Name");
            ViewData["Category"] = new SelectList(_context.Categories, "CategoryID", "Name");
            return View(promotion);
        }

        [HttpGet]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            ViewData["Product"] = new SelectList(_context.Products, "ProductID", "Name");
            ViewData["Category"] = new SelectList(_context.Categories, "CategoryID", "Name");
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Edit(int id, Promotion promotion)
        {
            if (id != promotion.PromotionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promotion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionExists(promotion.PromotionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Product"] = new SelectList(_context.Products, "ProductID", "Name");
            ViewData["Category"] = new SelectList(_context.Categories, "CategoryID", "Name");
            return View(promotion);
        }

        [HttpGet]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(m => m.PromotionID == id);
            if (promotion == null)
            {
                return NotFound();
            }

            return View(promotion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PromotionExists(int id)
        {
            return _context.Promotions.Any(e => e.PromotionID == id);
        }
    }
}
