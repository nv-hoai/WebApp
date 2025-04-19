using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Data;
using FastFood.MVC.Models;
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
            if ((await _authorization.AuthorizeAsync(User, "OrderManagementAccess")).Succeeded)
            {
                var managementOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Employee)
                    .Include(o => o.Shipper)
                    .ToListAsync();
                return View(managementOrders);
            }

            var customerOrders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.Customer.UserID == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .ToListAsync();

            return View(customerOrders);
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
    }
}
