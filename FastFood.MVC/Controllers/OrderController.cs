using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Data;
using FastFood.MVC.Models;
using FastFood.MVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Security.Cryptography;

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
        [Authorize(Policy = "CustomerEmployeeAdminAccess")]
		public async Task<IActionResult> CreateFromCart()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID);
			var employee = await _context.Employees.FirstOrDefaultAsync(s => s.UserID == userID);
			if (customer == null && employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "Cart");
			if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            //Tạo đơn hàng
            var order = new Order
            {
                CustomerID = customer.CustomerID,
                EmployeeID = employee?.EmployeeID,
                CreatedAt = DateTime.Now,
                Status = OrderStatus.Pending
            };

            if (!order.OrderDetails.Any())
            {
                return RedirectToAction("Index", "Cart");
            }
            //Tạo chi tiết đơn hàng
            foreach(var item in cartItems)
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductID == item.ProductID);
				if (product == null) continue;

				var orderDetail = new OrderDetail
				{
					ProductID = item.ProductID,
                    UnitPrice = item.Price,
					Quantity = item.Quantity,
					Product = product,
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
                orderDetail.CalculateSubTotal();
                order.OrderDetails.Add(orderDetail);
			}

			// Tính tổng tiền
			order.CalculateTotalCharge();

			_context.Orders.Add(order);
			await _context.SaveChangesAsync();

			// Xóa giỏ hàng
			HttpContext.Session.Remove("Cart");

			return RedirectToAction("Details", new { id = order.OrderID });
		}

        [Authorize(Policy = "CustomerEmployeeAdminAccess")]
        public async Task<IActionResult> Cancel(int orderID)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync (c => c.UserID == userID);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == userID);

            Order? order = null;

			if (customer != null)
            {
                order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.CustomerID == customer.CustomerID && o.OrderID == orderID && o.Status == OrderStatus.Pending);
            } else if (employee != null)
            {
                order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderID == orderID && o.Status == OrderStatus.Pending);
            }
            
            if (order == null)
            {
                TempData["ErrorMessage"] = $"Hệ thống không tìm thấy đơn hàng #{orderID} hoặc bạn không có quyền hủy. Vui lòng kiểm tra lại trước khi thao tác!";
            }
            else
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
				TempData["SuccessMessage"] = $"Đơn hàng #{orderID} đã được hủy thành công.";
			}

			return RedirectToAction("MyOrders");
		}

        [Authorize(Policy = "CustomerAccess")] 
        public async Task<IActionResult> MyOrders()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Where(o => o.CustomerID == customer.CustomerID)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [Authorize(Policy = "OrderManagementAccess")]
        public async Task<IActionResult> ChangeStatus(int orderID, OrderStatus status)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == userID);
			var shipper = await _context.Shippers.FirstOrDefaultAsync(e => e.UserID == userID);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderID);
            if (order == null)
            {
				TempData["ErrorMessage"] = $"Không tìm thấy đơn hàng #{orderID}.";
				return RedirectToAction("Index", "Order");
			}

			bool isAuthorized = false;
			if (shipper != null && status == OrderStatus.Delivering)
				isAuthorized = true;
			else if (employee != null && status != OrderStatus.Cancelled)
				isAuthorized = true;

			if (!isAuthorized)
			{
				TempData["ErrorMessage"] = $"Bạn không có quyền thay đổi trạng thái đơn hàng thành '{status}'.";
				return RedirectToAction("Index", "Order");
			}

			order.Status = status;
			_context.Update(order);
            _context.SaveChangesAsync();
			TempData["SuccessMessage"] = $"Thành công thay đổi trạng thái đơn hàng #{orderID} thành '{status}'.";

			return RedirectToAction("Index", "Order");
		}

    }
}
