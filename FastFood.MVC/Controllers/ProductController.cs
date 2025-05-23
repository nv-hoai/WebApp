﻿using Microsoft.AspNetCore.Mvc;
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
        private readonly NotificationService _notificationService;

        public ProductController(
            ApplicationDbContext context,
            AzureBlobService blobService,
            NotificationService notificationService)
        {
            _context = context;
            _blobService = blobService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(string? category, string? productName, string? priceSort, int? activeIndex)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["Category"] = new SelectList(categories, "Name", "Name", category);

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsCarouselItem)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.Name == category);
            }

            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(p => p.Name.Contains(productName));
            }

            switch (priceSort)
            {
                case "asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }

            var products = await query.ToListAsync();
            var model = new ProductIndexViewModel(categories, products.AsEnumerable());
            ViewData["activeIndex"] = activeIndex ?? 0;

            return View(model);
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
                var productAdded = await _context.Products
                    .FirstOrDefaultAsync(m => m.ProductID == product.ProductID);
                var customers = await _context.Customers.ToListAsync();
                foreach (var customer in customers)
                {
                    await _notificationService.CreateNotification(
                        customer.UserID,
                        $"Có sản phẩm mới, xem ngay!",
                        $"/Product/Details/{productAdded.ProductID}",
                        "fa-check-circle");
                }
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

            ProductViewModel model = new ProductViewModel()
            {
                ProductID = product.ProductID,
                Name = product.Name,
                CategoryID = product.CategoryID,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                IsNew = product.IsNew,
                IsCarouselItem = product.IsCarouselItem,
                IsPopular = product.IsPopular
            };

            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "Name", product.CategoryID);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                Product product = new Product()
                {
                    ProductID = model.ProductID,
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
                else
                {
                    product.ImageUrl = model.ImageUrl;
                }

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
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "Name", model.CategoryID);
            return View(model);
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
