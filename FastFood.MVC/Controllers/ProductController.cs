using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Data;
using FastFood.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using FastFood.MVC.ViewModels;
using FastFood.MVC.Services;

namespace FastFood.MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AzureBlobService _blobService;

        public ProductController(ApplicationDbContext context, AzureBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index(string? category)
        {
            List<Product> products;

            if (category == null) 
            {
                products = await _context.Products
                    .Where(p => !p.IsCarouselItem)
                    .Include(p => p.Category)
                    .ToListAsync();
                return View(products);
            }

            products = await _context.Products
                .Where(p => !p.IsCarouselItem)
                .Include(p => p.Category)
                .Where(products => products.Category.Name == category)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [Authorize(Policy = "AdminAccess")]
        public IActionResult Create()
        {
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "Name");
            ProductViewModel model = new ProductViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Product product = new Product()
                {
                    Name = model.Name,
                    CategoryID = model.CategoryID,
                    Description = model.Description,
                    Price = model.Price,
                    IsNew = model.IsNew,
                    IsCarouselItem = model.IsCarouselItem,
                    IsPopular = model.IsPopular
                };

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    product.ImageUrl = await _blobService.UploadFileAsync(model.ImageFile);
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "Name", model.CategoryID);
            return View(model);
        }

        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "Name", product.CategoryID);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Edit(int id, ProductViewModel product)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
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
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "Name", product.CategoryID);
            return View(product);
        }

        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}
