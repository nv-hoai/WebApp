using System.Reflection.Metadata.Ecma335;
using FastFood.MVC.Data;
using FastFood.MVC.Helpers;
using FastFood.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

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

        private List<CartItem> GetCart()
        {
            var cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");
            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            return cart;
        }

        private void SaveCart(List<CartItem> cart)
        {
            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
        }

        //Hiển thị giỏ hàng
        [HttpGet]
        public IActionResult Index()
        {
            return View(GetCart());
        }

        //Thêm sản phẩm vào giỏ
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productID, int quantity = 1)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
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
            var cart = GetCart();
            var product = _context.Products.Find(productID);
            if (product == null)
            {
                return NotFound();
            }
            var existingItem = cart.FirstOrDefault(c => c.ProductID == productID);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                decimal discountPercent = 0;
                int? promotionID = null;
                var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.ProductID == productID);

                if (promotion != null)
                {
                    discountPercent = promotion.DiscountPercent;
                    promotionID = promotion.PromotionID;
                }

                cart.Add(new CartItem
                {
                    ProductID = productID,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    DiscountPercent = discountPercent,
                    PromotionID = promotionID
                });
            }
            SaveCart(cart);
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng!" });
        }

        //Xóa sản phẩm khỏi giỏ
        public IActionResult RemoveFromCart(int productID)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == productID);
            if (item != null)
            {
                cart.Remove(item);
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        //Cập nhật số lượng sản phẩm
        [HttpPost]
        public async Task<IActionResult> UpdateCart(int productID, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == productID);

            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        //Xóa giỏ hàng
        public async Task<IActionResult> ClearCart()
        {
            SaveCart(new List<CartItem>());
            return RedirectToAction("Index");
        }
    }
}
