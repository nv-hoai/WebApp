using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Data;
using FastFood.MVC.Models;
using FastFood.MVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            // For Admin and Employee: Show all orders
            if ((await _authorization.AuthorizeAsync(User, "AdminOrEmployeeAccess")).Succeeded)
            {
                var managementOrders = await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Employee)
                        .ThenInclude(e => e.User)
                    .Include(o => o.Shipper)
                        .ThenInclude(s => s.User)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
                return View(managementOrders.AsEnumerable());
            }

            // For Shipper: Show only orders that are Prepared or ones they are delivering
            if ((await _authorization.AuthorizeAsync(User, "ShipperAccess")).Succeeded)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var shipper = await _context.Shippers.FirstOrDefaultAsync(s => s.UserID == userId);

                if (shipper == null)
                {
                    return NotFound("Shipper profile not found");
                }

                var shipperOrders = await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Employee)
                        .ThenInclude(e => e.User)
                    .Include(o => o.Shipper)
                        .ThenInclude(e => e.User)
                    .Where(o => o.Status == OrderStatus.Prepared ||
                               ((o.Status == OrderStatus.Delivering || o.Status == OrderStatus.Completed) 
                                    && o.ShipperID == shipper.ShipperID))
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return View(shipperOrders.AsEnumerable());
            }

            // For Customer: Show their own orders
            var customerOrders = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Where(o => o.Customer.UserID == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(customerOrders.AsEnumerable());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // For Admin, Employee: Can view any order details
            if ((await _authorization.AuthorizeAsync(User, "AdminOrEmployeeAccess")).Succeeded)
            {
                var managementOrder = await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Employee)
                        .ThenInclude(e => e.User)
                    .Include(o => o.Shipper)
                        .ThenInclude(s => s.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(o => o.Product)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(o => o.Promotion)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (managementOrder == null)
                {
                    return NotFound();
                }

                return View(managementOrder);
            }

            // For Shipper: Can view orders they're delivering or that are ready for pickup (Processing)
            if ((await _authorization.AuthorizeAsync(User, "ShipperAccess")).Succeeded)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var shipper = await _context.Shippers.FirstOrDefaultAsync(s => s.UserID == userId);

                if (shipper == null)
                {
                    return NotFound("Shipper profile not found");
                }

                var shipperOrder = await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Employee)
                        .ThenInclude(e => e.User)
                    .Include(o => o.Shipper)
                        .ThenInclude(e => e.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(o => o.Product)
                    .FirstOrDefaultAsync(o => o.OrderID == id &&
                                          (o.Status == OrderStatus.Processing ||
                                          (o.Status == OrderStatus.Delivering && o.ShipperID == shipper.ShipperID)));

                if (shipperOrder == null)
                {
                    return NotFound("Order not found or you don't have permission to view it");
                }

                return View(shipperOrder);
            }

            // For Customer: Can only view their own orders
            var customerOrder = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(o => o.Product)
                .FirstOrDefaultAsync(o => o.Customer.UserID == User.FindFirstValue(ClaimTypes.NameIdentifier) && o.OrderID == id);

            if (customerOrder == null)
            {
                return NotFound();
            }

            return View(customerOrder);
        }

        [Authorize(Policy = "AdminAccess")]
        public IActionResult Create()
        {
            Order model = new Order();
            ViewData["CustomerID"] = new SelectList(_context.Customers, "CustomerID", "CustomerID");
            ViewData["EmployeeID"] = new SelectList(_context.Employees, "EmployeeID", "EmployeeID");
            ViewData["ShipperID"] = new SelectList(_context.Shippers, "ShipperID", "ShipperID");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Create(Order order)
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

        [Authorize(Policy = "AdminAccess")]
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
        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Edit(int id, Order order)
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
		public async Task<IActionResult> CreateFromCart(string Address, ShippingMethod ShippingMethod)
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

            var carts = await _context.CartItems
                                    .Include(c => c.Promotion)
                                    .Where(c => c.CustomerID == customer.CustomerID)
                                    .ToListAsync();

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
				var orderDetail = new OrderDetail
				{
                    OrderID = order.OrderID,
					ProductID = item.ProductID,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
					Quantity = item.Quantity,
                    PromotionID = item.PromotionID,
                    PromotionName = item.PromotionName,
                    Promotion = item.Promotion,
                };

                orderDetail.CalculatePrices();
                order.OrderDetails.Add(orderDetail);
			}

			order.CalculateTotalCharge();

			_context.Orders.Add(order);
            _context.CartItems.RemoveRange(carts);
			await _context.SaveChangesAsync();

			return Json(new 
            { 
                success = true
            });
		}

        // AJAX endpoint for cancelling an order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int orderID)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Order? order = null;

            // Admin or Employee can cancel any order
            if ((await _authorization.AuthorizeAsync(User, "AdminOrEmployeeAccess")).Succeeded)
            {
                order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderID);
                if (order == null)
                {
                    return Json(new { success = false, message = $"Không tìm thấy đơn hàng #{orderID}." });
                }
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = $"Đơn hàng #{orderID} đã được hủy."
                });
            }

            // Customers can only cancel their own orders in Pending state
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userID);
            if (customer == null)
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện chức năng này" });
            }

            order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderID && o.CustomerID == customer.CustomerID);

            if (order == null)
            {
                return Json(new { success = false, message = $"Không tìm thấy đơn hàng #{orderID}." });
            }

            if (order.Status != OrderStatus.Pending)
            {
                return Json(new
                {
                    success = false,
                    message = $"Không thể hủy đơn hàng #{orderID}."
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EmployeeAccess")]
        public async Task<IActionResult> AcceptOrder(int orderID)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == userID);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderID && o.Status == OrderStatus.Pending);

            if (order == null)
            {
                return Json(new { success = false, message = $"Order #{orderID} not found or is not in Pending state" });
            }

            // Assign the employee to the order and update status
            order.EmployeeID = employee!.EmployeeID;
            order.Status = OrderStatus.Processing;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Order #{orderID} has been accepted and is now being processed" });
        }

        // Employee marks order as ready for delivery: Processing -> Prepared
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EmployeeAccess")]
        public async Task<IActionResult> MarkAsPrepared(int orderID)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderID && o.Status == OrderStatus.Processing);

            if (order == null)
            {
                return Json(new { success = false, message = $"Order #{orderID} not found or is not in Processing state" });
            }

            // Update status
            order.Status = OrderStatus.Prepared;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Order #{orderID} is now ready for delivery" });
        }

        // Shipper accepts the order for delivery: Prepared -> Delivering
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ShipperAccess")]
        public async Task<IActionResult> AcceptDelivery(int orderID)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var shipper = await _context.Shippers.FirstOrDefaultAsync(s => s.UserID == userID);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderID && o.Status == OrderStatus.Prepared);

            if (order == null)
            {
                return Json(new { success = false, message = $"Order #{orderID} not found or is not ready for delivery" });
            }

            // Assign the shipper to the order and update status
            order.ShipperID = shipper!.ShipperID;
            order.Status = OrderStatus.Delivering;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"You have accepted order #{orderID} for delivery" });
        }

        // Shipper marks the order as delivered: Delivering -> Completed
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ShipperAccess")]
        public async Task<IActionResult> MarkAsDelivered(int orderID)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var shipper = await _context.Shippers.FirstOrDefaultAsync(s => s.UserID == userID);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == orderID &&
                                                               o.Status == OrderStatus.Delivering &&
                                                               o.ShipperID == shipper!.ShipperID);
            
            if (order == null)
            {
                return Json(new { success = false, message = $"Order #{orderID} not found or not assigned to you for delivery" });
            }


            var orderDetails = await _context.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderID == orderID)
                .ToListAsync();

            foreach (var orderDetail in orderDetails)
            {
                var product = await _context.Products.FindAsync(orderDetail.ProductID);
                if (product != null)
                {
                    product.SoldQuantity += orderDetail.Quantity;
                }
            }

            // Update status and completion time
            order.Status = OrderStatus.Completed;
            order.CompletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Order #{orderID} has been marked as delivered" });
        }
    }
}
