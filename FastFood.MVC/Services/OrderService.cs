using FastFood.MVC.Data;
using FastFood.MVC.Models;
using FastFood.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FastFood.MVC.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public OrderService(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(OrderQueryParameters parameters)
        {
            var query = _context.Orders.AsQueryable();
            
            // Include related entities
            query = query.Include(o => o.Customer)
                          .ThenInclude(c => c.User);
                          
            if (parameters.IsAdmin || parameters.IsEmployee || parameters.IsShipper)
            {
                query = query.Include(o => o.Employee)
                            .ThenInclude(e => e.User)
                            .Include(o => o.Shipper)
                            .ThenInclude(s => s.User);
            }

            // Apply filters based on user role
            if (!parameters.IsAdmin && !parameters.IsEmployee)
            {
                if (parameters.IsShipper && parameters.ShipperId.HasValue)
                {
                    // Shipper can see prepared orders and orders they're delivering/delivered
                    query = query.Where(o => 
                        o.Status == OrderStatus.Prepared || 
                        ((o.Status == OrderStatus.Delivering || o.Status == OrderStatus.Completed) 
                            && o.ShipperID == parameters.ShipperId));
                }
                else
                {
                    // Customer can only see their own orders
                    query = query.Where(o => o.Customer.UserID == parameters.UserId);
                }
            }

            // Apply common filters
            if (parameters.OrderId.HasValue)
            {
                query = query.Where(o => o.OrderID == parameters.OrderId.Value);
            }

            if (parameters.Status.HasValue)
            {
                // For shippers, we need special status filtering
                if (parameters.IsShipper && parameters.ShipperId.HasValue)
                {
                    if (parameters.Status == OrderStatus.Prepared)
                    {
                        query = query.Where(o => o.Status == OrderStatus.Prepared);
                    }
                    else
                    {
                        query = query.Where(o => o.Status == parameters.Status && o.ShipperID == parameters.ShipperId);
                    }
                }
                else
                {
                    query = query.Where(o => o.Status == parameters.Status);
                }
            }

            if (!string.IsNullOrEmpty(parameters.CustomerName) && (parameters.IsAdmin || parameters.IsEmployee || parameters.IsShipper))
            {
                query = query.Where(o => o.Customer.User.FullName.Contains(parameters.CustomerName));
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date >= parameters.StartDate.Value.Date);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date <= parameters.EndDate.Value.Date);
            }

            // Order by creation date, newest first
            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id, string userId, bool isAdmin = false, bool isEmployee = false, bool isShipper = false)
        {
            if (isAdmin || isEmployee)
            {
                // Admin/Employee can see all order details
                return await _context.Orders
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
            }
            
            if (isShipper)
            {
                var shipper = await _context.Shippers.FirstOrDefaultAsync(s => s.UserID == userId);
                if (shipper == null) return null;
                
                // Shipper can see orders that are prepared or that they are delivering
                return await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Employee)
                        .ThenInclude(e => e.User)
                    .Include(o => o.Shipper)
                        .ThenInclude(s => s.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(o => o.Product)
                    .FirstOrDefaultAsync(o => 
                        o.OrderID == id && (
                            o.Status == OrderStatus.Prepared ||
                            (o.Status == OrderStatus.Delivering && o.ShipperID == shipper.ShipperID)
                        ));
            }
            
            // Customer can only see their own orders
            return await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(o => o.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.Customer.UserID == userId);
        }

        public async Task<bool> OrderExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }

        public async Task<Order> CreateOrderFromCartAsync(int customerId, string address, ShippingMethod shippingMethod)
        {
            var carts = await _context.CartItems
                .Include(c => c.Promotion)
                .Where(c => c.CustomerID == customerId)
                .ToListAsync();

            if (carts == null || !carts.Any())
            {
                return null;
            }

            var order = new Order
            {
                CustomerID = customerId,
                Address = address,
                ShippingMethod = shippingMethod,
                CreatedAt = DateTime.Now,
                Status = OrderStatus.Pending
            };

            foreach (var item in carts)
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

            return order;
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId, string note, bool isAdminOrEmployee = false)
        {
            Order order;

            if (isAdminOrEmployee)
            {
                order = await _context.Orders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OrderID == orderId);
            }
            else
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
                if (customer == null) return false;

                order = await _context.Orders.FirstOrDefaultAsync(o => 
                    o.OrderID == orderId && o.CustomerID == customer.CustomerID && o.Status == OrderStatus.Pending);
            }

            if (order == null) return false;

            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.Now;
            order.Note = note;

            if (isAdminOrEmployee)
            {
                await _notificationService.CreateNotification(
                    order.Customer.UserID,
                    $"Đơn hàng #{orderId} đã bị hủy với lí do {note}.",
                    $"/Order/Details/{orderId}",
                    "fa-check-circle"
                );
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AcceptOrderAsync(int orderId, int employeeId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderID == orderId && o.Status == OrderStatus.Pending);

            if (order == null) return false;

            await _notificationService.CreateNotification(
                order.Customer.UserID,
                $"Đơn hàng #{orderId} đã được xác nhận và đang chuẩn bị.",
                $"/Order/Details/{orderId}",
                "fa-check-circle"
            );

            order.EmployeeID = employeeId;
            order.Status = OrderStatus.Processing;
            order.ProcessingAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkOrderAsPreparedAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderID == orderId && o.Status == OrderStatus.Processing);

            if (order == null) return false;

            await _notificationService.CreateNotification(
                order.Customer.UserID,
                $"Đơn hàng #{orderId} đã được chuẩn bị xong và sắp được giao.",
                $"/Order/Details/{orderId}",
                "fa-check-circle"
            );

            var shippers = await _context.Shippers.ToListAsync();
            foreach (var shipper in shippers)
            {
                await _notificationService.CreateNotification(
                    shipper.UserID,
                    $"Có đơn hàng #{orderId} có thể nhận.",
                    $"/Order/Details/{orderId}",
                    "fa-check-circle"
                );
            }

            order.Status = OrderStatus.Prepared;
            order.PreparedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AcceptDeliveryAsync(int orderId, int shipperId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(o => o.OrderID == orderId && o.Status == OrderStatus.Prepared);

            if (order == null) return false;

            await _notificationService.CreateNotification(
                order.Customer.UserID,
                $"Đơn hàng #{orderId} đang được vận chuyển đến khách hàng.",
                $"/Order/Details/{orderId}",
                "fa-check-circle"
            );

            await _notificationService.CreateNotification(
                order.Employee!.UserID,
                $"Đơn hàng #{orderId} đang được vận chuyển đến khách hàng.",
                $"/Order/Details/{orderId}",
                "fa-check-circle"
            );

            order.ShipperID = shipperId;
            order.Status = OrderStatus.Delivering;
            order.DeliveringAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkOrderAsDeliveredAsync(int orderId, int shipperId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(o => 
                    o.OrderID == orderId &&
                    o.Status == OrderStatus.Delivering &&
                    o.ShipperID == shipperId);

            if (order == null) return false;

            await _notificationService.CreateNotification(
                order.Customer.UserID,
                $"Đơn hàng #{orderId} đã hoàn thành.",
                $"/Order/Details/{orderId}",
                "fa-check-circle"
            );

            await _notificationService.CreateNotification(
                order.Employee!.UserID,
                $"Đơn hàng #{orderId} đã hoàn thành.",
                $"/Order/Details/{orderId}",
                "fa-check-circle"
            );

            var orderDetails = await _context.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderID == orderId)
                .ToListAsync();

            foreach (var orderDetail in orderDetails)
            {
                var product = await _context.Products.FindAsync(orderDetail.ProductID);
                if (product != null)
                {
                    product.SoldQuantity += orderDetail.Quantity;
                }
            }

            order.Status = OrderStatus.Completed;
            order.CompletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await OrderExistsAsync(order.OrderID))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;
            
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
