using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Data;
using FastFood.MVC.Models;
using FastFood.MVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FastFood.MVC.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorization;

        public OrderController(ApplicationDbContext context, IAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        public async Task<IActionResult> Index()
        {
            if ((await _authorization.AuthorizeAsync(User, "OrderManagementAccess")).Succeeded)
            {
                var managementOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Employee)
                    .Include(o => o.Shipper)
                    .ToListAsync();
                return View(managementOrders.AsEnumerable());
            }

            var customerOrders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.Customer.UserID == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .ToListAsync();

            return View(customerOrders.AsEnumerable());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [Authorize(Policy = "AdminOrEmployeeAccess")]
        public IActionResult Create()
        {
            ViewData["CustomerID"] = new SelectList(_context.Customers, "CustomerID", "CustomerID");
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "EmployeeID");
            ViewData["ShipperID"] = new SelectList(_context.Shippers, "ShipperID", "ShipperID");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrEmployeeAccess")]
        public async Task<IActionResult> Create([Bind("OrderID,CustomerID,ShipperID,EmployeeID,TotalCharge,Status,CreatedAt,CompletedAt")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerID"] = new SelectList(_context.Customers, "CustomerID", "CustomerID", order.CustomerID);
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "EmployeeID", order.EmployeeID);
            ViewData["ShipperID"] = new SelectList(_context.Shippers, "ShipperID", "ShipperID", order.ShipperID);
            return View(order);
        }

        [Authorize(Policy = "OrderManagementAcess")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerID"] = new SelectList(_context.Customers, "CustomerID", "CustomerID", order.CustomerID);
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "EmployeeID", order.EmployeeID);
            ViewData["ShipperID"] = new SelectList(_context.Shippers, "ShipperID", "ShipperID", order.ShipperID);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "OrderManagementAcess")]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,CustomerID,ShipperID,EmployeeID,TotalCharge,Status,CreatedAt,CompletedAt")] Order order)
        {
            if (id != order.OrderID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderID))
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
            ViewData["CustomerID"] = new SelectList(_context.Customers, "CustomerID", "CustomerID", order.CustomerID);
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "EmployeeID", order.EmployeeID);
            ViewData["ShipperID"] = new SelectList(_context.Shippers, "ShipperID", "ShipperID", order.ShipperID);
            return View(order);
        }

        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CustomerAccess")]
		public async Task<IActionResult> CreateFromCart(String Address, ShippingMethod ShippingMethod)
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

            var carts = await _context.CartItems.Where(c => c.CustomerID == customer.CustomerID).ToListAsync();
			if (carts == null || !carts.Any())
            {
				return Json(new 
                { 
                    success = false, 
                    message = "Giỏ hàng trống." 
                });
			}

            //Tạo đơn hàng
            var order = new Order
            {
                CustomerID = customer.CustomerID,
                Address = Address,
                ShippingMethod = ShippingMethod,
                CreatedAt = DateTime.Now,
                Status = OrderStatus.Pending
            };

            //Tạo chi tiết đơn hàng
            foreach(var item in carts)
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductID == item.ProductID);
				if (product == null) continue;

				var orderDetail = new OrderDetail
				{
                    OrderID = order.OrderID,
					ProductID = item.ProductID,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
					Quantity = item.Quantity,
				};

                Promotion? promo = null;
                if (item.PromotionID.HasValue)
                {
                    promo = await _context.Promotions
                        .FirstOrDefaultAsync(p => p.PromotionID == item.PromotionID.Value
                                                && p.StartDate <= DateTime.Now
                                                && p.ExpiryDate >= DateTime.Now);
                }
                else
                {
                    promo = await _context.Promotions
                        .Where(p => (p.ProductID == product.ProductID || p.CategoryID == product.CategoryID)
                        && p.StartDate <= DateTime.Now
                        && p.ExpiryDate >= DateTime.Now)
                        .OrderByDescending(p => p.DiscountPercent)
                        .FirstOrDefaultAsync();
				}

                orderDetail.PromotionID = promo?.PromotionID;
                orderDetail.CalculatePrices();
                order.OrderDetails.Add(orderDetail);
			}

			if (!order.OrderDetails.Any())
			{
				return Json(new 
                { 
                    success = false, 
                    message = "Không có sản phẩm hợp lệ trong giỏ hàng." 
                });
			}

			order.CalculateTotalCharge();

			_context.Orders.Add(order);
            _context.CartItems.RemoveRange(carts);
			await _context.SaveChangesAsync();

			return Json(new 
            { 
                success = true, 
                orderId = order.OrderID 
            });
		}

		[Authorize(Policy = "CustomerAccess")]
		public async Task<IActionResult> Cancel(int orderID)
		{
			var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID);

			if (customer == null)
			{
				return Json(new { success = false, message = "Bạn chưa đăng nhập." });
			}

			var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderID && o.CustomerID == customer.CustomerID);

			if (order == null)
			{
				return Json(new { success = false, message = $"Không tìm thấy đơn hàng #{orderID}." });
			}

			if (order.Status != OrderStatus.Pending)
			{
				return Json(new
				{
					success = false,
					message = $"Không thể hủy đơn hàng #{orderID} vì trạng thái không phải 'Mới tạo'."
				});
			}

			order.Status = OrderStatus.Cancelled;

			await _context.SaveChangesAsync();

			return Json(new
			{
				success = true,
				message = $"Đơn hàng #{orderID} đã được hủy."
			});
		}

		[Authorize(Policy = "CustomerAccess")] 
        public async Task<IActionResult> MyOrders()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID);

			if (customer == null)
			{
				return RedirectToPage("/Account/Login", new
				{
					area = "Identity",
					returnUrl = Url.Action("MyOrders", "Order")
				});
			}

			var orders = await _context.Orders
		        .Where(o => o.CustomerID == customer.CustomerID)
		        .Include(o => o.OrderDetails)
		        .OrderByDescending(o => o.CreatedAt)
		        .ToListAsync();

            foreach (var order in orders)
            {
                order.CalculateTotalCharge();
            }

			return View(orders);
        }

		[Authorize(Policy = "CustomerAccess")]
		public async Task<IActionResult> MyOrderDetails(int OrderID)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userId);

			if (customer == null)
			{
				return RedirectToPage("/Account/Login", new
				{
					area = "Identity",
					returnUrl = Url.Action("MyOrderDetails", "Order", new { OrderID })
				});
			}

			var order = await _context.Orders
				.Include(o => o.OrderDetails)
					.ThenInclude(od => od.Product)
				.FirstOrDefaultAsync(o => o.OrderID == OrderID && o.CustomerID == customer.CustomerID);

			if (order == null)
			{
				return NotFound();
			}

			order.CalculateTotalCharge(); // Tính lại nếu cần
			return View(order);
		}

	}
}
