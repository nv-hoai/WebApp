using FastFood.MVC.Models;

namespace FastFood.MVC.ViewModels
{
    public class DashboardViewModel
    {
        public int TodayOrdersCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewCustomersCount { get; set; }
        public string TopSellingProductName { get; set; } = string.Empty;
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
        public List<DailyRevenueViewModel> Last7DaysRevenue { get; set; } = new();
    }
    
    public class RecentOrderViewModel
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderTime { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
    }
    
    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }
}
