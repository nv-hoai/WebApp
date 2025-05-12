using FastFood.MVC.Data;
using FastFood.MVC.Models;
using FastFood.MVC.ViewModels;
using Microsoft.EntityFrameworkCore;
using FastFood.MVC.Services.Interfaces;

namespace FastFood.MVC.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        
        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var today = DateTime.Today;
            var weekAgo = today.AddDays(-7);
            
            // Get today's orders count
            var todayOrdersCount = await _context.Orders
                .CountAsync(o => o.CreatedAt.Date == today);
                
            // Get total revenue
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalCharge) ?? 0;
                
            // Get new customers count from the past 7 days
            var newCustomersCount = await _context.Customers
                .CountAsync(c => c.User.CreatedAt >= weekAgo);
                
            // Get top selling product
            var topSellingProduct = await _context.OrderDetails
                .GroupBy(od => od.ProductID)
                .Select(g => new { 
                    ProductID = g.Key,
                    ProductName = g.First().ProductName,
                    TotalQuantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .FirstOrDefaultAsync();
                
            // Get recent orders 
            var recentOrders = await _context.Orders
                .Include(o => o.Customer.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderViewModel {
                    OrderID = o.OrderID,
                    CustomerName = o.Customer.User.FullName,
                    OrderTime = o.CreatedAt,
                    TotalAmount = o.TotalCharge ?? 0,
                    Status = o.Status
                })
                .ToListAsync();
                
            // Get last 7 days revenue data for chart
            var last7DaysRevenue = await GetLast7DaysRevenueAsync();
            
            return new DashboardViewModel
            {
                TodayOrdersCount = todayOrdersCount,
                TotalRevenue = totalRevenue,
                NewCustomersCount = newCustomersCount,
                TopSellingProductName = topSellingProduct?.ProductName ?? "N/A",
                RecentOrders = recentOrders,
                Last7DaysRevenue = last7DaysRevenue
            };
        }
        
        private async Task<List<DailyRevenueViewModel>> GetLast7DaysRevenueAsync()
        {
            var today = DateTime.Today;
            var result = new List<DailyRevenueViewModel>();
            
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var revenue = await _context.Orders
                    .Where(o => o.CreatedAt.Date == date && o.Status == OrderStatus.Completed)
                    .SumAsync(o => o.TotalCharge) ?? 0;
                    
                result.Add(new DailyRevenueViewModel
                {
                    Date = date,
                    Revenue = revenue
                });
            }
            
            return result;
        }
    }
}
