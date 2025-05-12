using FastFood.MVC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FastFood.MVC.Services.Interfaces
{
    public interface IOrderService
    {
        // Query methods
        Task<IEnumerable<Order>> GetOrdersAsync(OrderQueryParameters parameters);
        Task<Order> GetOrderByIdAsync(int id, string userId, bool isAdmin = false, bool isEmployee = false, bool isShipper = false);
        Task<bool> OrderExistsAsync(int id);
        
        // Command methods
        Task<Order> CreateOrderFromCartAsync(int customerId, string address, ShippingMethod shippingMethod);
        Task<bool> CancelOrderAsync(int orderId, string userId, string note, bool isAdminOrEmployee = false);
        Task<bool> AcceptOrderAsync(int orderId, int employeeId);
        Task<bool> MarkOrderAsPreparedAsync(int orderId);
        Task<bool> AcceptDeliveryAsync(int orderId, int shipperId);
        Task<bool> MarkOrderAsDeliveredAsync(int orderId, int shipperId);
        
        // Admin operations
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int orderId);
    }
    
    public class OrderQueryParameters
    {
        public int? OrderId { get; set; }
        public string UserId { get; set; }
        public OrderStatus? Status { get; set; }
        public string CustomerName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsEmployee { get; set; }
        public bool IsShipper { get; set; }
        public int? ShipperId { get; set; }
    }
}
