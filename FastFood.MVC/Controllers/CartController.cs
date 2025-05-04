using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using FastFood.MVC.Data;
using FastFood.MVC.Helpers;
using FastFood.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using NuGet.Protocol.Plugins;
using System.Collections.Generic;

namespace FastFood.MVC.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorization;

        public CartController(ApplicationDbContext context, IAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        //Lấy thông tin giỏ hàng của khách ID từ DB
		public async Task<List<CartItem>> GetCartAsync(int CustomerID)
		{
			var cart = await _context.CartItems
									.Include(c => c.Product)
									.Where(c => c.CustomerID == CustomerID)
									.ToListAsync(); 
			return cart;
		}

        //Hiển thị giỏ hàng sau khi đăng nhập
        [HttpGet]
        [Authorize(Policy = "CustomerAccess")]
        public async Task<IActionResult> Index()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID);

			if (customer == null)
			{
				return RedirectToPage("/Account/Login", new
				{
					area = "Identity",
					returnUrl = Url.Action("Index", "Cart")
				});
			}

			var cart = await GetCartAsync(customer.CustomerID);
			return View(cart);
		}

        //Thêm sản phẩm vào giỏ
        [HttpPost]
        [Authorize(Policy = "CustomerAccess")]
        public async Task<IActionResult> AddToCart(int productID, int quantity = 1)
        {
			if (quantity < 1)
			{
				return Json(new 
                { 
                    success = false, 
                    message = "Số lượng sản phẩm phải lớn hơn 0." 
                });
			}

			var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID);

            if (customer == null)
            {
				return Json(new
				{
					success = false,
					redirectUrl = Url.Page("/Account/Login", new
					{
						area = "Identity",
						returnUrl = Url.Action("Index", "Product")
					}),
					message = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ."
				});
			}
            
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productID);
            if (product == null)
            {
                return Json (new {
                    success = false,
                    message = $"Không tìm thấy sản phẩm có mã #{productID}"
                });
            }

            var carts = await GetCartAsync(customer.CustomerID);
			var existingItem = carts.FirstOrDefault(c => c.ProductID == productID);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
				existingItem.Calculate(); 
				_context.CartItems.Update(existingItem);
			}
            else
            {
                var promotion = await _context.Promotions.FirstOrDefaultAsync
                (
                        p => p.ProductID == productID && 
                             p.CategoryID == product.CategoryID
                );

                var cartItem = new CartItem
                {
                    CustomerID = customer.CustomerID,
                    ProductID = productID,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = quantity,
                    PromotionID = promotion?.PromotionID,
                    PromotionName = promotion?.Name,
					CreatedAt = DateTime.Now,
					Product = product
				};

				cartItem.Calculate();
				await _context.CartItems.AddAsync(cartItem);

			}
            await _context.SaveChangesAsync();
			return Json(new 
            { 
                success = true, 
                message = $"Đã thêm sản phẩm {productID} vào giỏ hàng!",
				cartCount = carts.Sum(c => c.Quantity)
			});
        }

        //Xóa sản phẩm khỏi giỏ
        [Authorize (Policy = "CustomerAccess")]
        public async Task<IActionResult> RemoveFromCart(int productID)
        {
			var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID); 
            
            if (customer == null)
            {
				return Json(new
				{
					success = false,
					redirectUrl = Url.Page("/Account/Login", new
					{
						area = "Identity",
						returnUrl = Url.Action("Index", "Product")
					}),
					message = "Bạn cần đăng nhập để xóa sản phẩm khỏi giỏ."
				});
			}

            var carts = await GetCartAsync(customer.CustomerID);
            var cartItem = carts.FirstOrDefault(ci => ci.ProductID == productID);
			if (cartItem == null)
			{
				return Json(new
				{
					success = false,
					message = $"Không tìm thấy sản phẩm mã #{productID} trong giỏ hàng."
				});
			}

			_context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

			return Json(new
			{
				success = true,
				message = $"Xóa thành công sản phẩm mã #{productID} khỏi giỏ hàng!"
			});
		}

		//Cập nhật số lượng sản phẩm
		[HttpPost]
		[Authorize(Policy = "CustomerAccess")]
		public async Task<IActionResult> UpdateCart(int productID, int quantity)
		{
			var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID);

			if (customer == null)
			{
				return Json(new
				{
					success = false,
					redirectUrl = Url.Page("/Account/Login", new
					{
						area = "Identity",
						returnUrl = Url.Action("Index", "Product")
					}),
					message = "Bạn cần đăng nhập để cập nhật giỏ hàng."
				});
			}

			var carts = await GetCartAsync(customer.CustomerID);
			var cartItem = carts.FirstOrDefault(ci => ci.ProductID == productID);

			if (cartItem == null)
			{
				return Json(new
				{
					success = false,
					message = $"Không tìm thấy sản phẩm mã #{productID} trong giỏ hàng."
				});
			}

			if (quantity <= 0)
			{
				_context.CartItems.Remove(cartItem);
				await _context.SaveChangesAsync();
				return Json(new
				{
					success = true,
					message = $"Đã xóa sản phẩm mã #{productID} vì số lượng nhỏ hơn hoặc bằng 0."
				});
			}

				cartItem.Quantity = quantity;
			_context.CartItems.Update(cartItem);
			await _context.SaveChangesAsync();

			return Json(new
			{
				success = true,
				message = $"Thành công thay đổi số lượng sản phẩm #{productID} thành {quantity}."
			});
		}

		//Xóa giỏ hàng
		[Authorize(Policy = "CustomerAccess")]
		public async Task<IActionResult> ClearCart()
        {
			var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID);

			if (customer == null)
			{
				return Json(new
				{
					success = false,
					redirectUrl = Url.Page("/Account/Login", new
					{
						area = "Identity",
						returnUrl = Url.Action("Index", "Product")
					}),
					message = "Bạn cần đăng nhập để cập nhật giỏ hàng."
				});
			}

			var carts = await GetCartAsync(customer.CustomerID);

			if (carts != null)
			{
				_context.CartItems.RemoveRange(carts);
			}

			await _context.SaveChangesAsync();
			return Json(new 
			{ 
				success = true,
				message = "Đã xóa thành công toàn bộ giỏ hàng",
				cartCount = 0
			});
        }
    }
}
