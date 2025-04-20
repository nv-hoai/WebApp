using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Data;
using FastFood.MVC.Models;
using FastFood.MVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using FastFood.MVC.Services;

namespace FastFood.MVC.Controllers
{
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AzureBlobService _blobService;

        public PromotionController(ApplicationDbContext context, AzureBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Promotions.ToListAsync());
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
