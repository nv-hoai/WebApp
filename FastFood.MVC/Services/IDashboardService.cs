using FastFood.MVC.ViewModels;

namespace FastFood.MVC.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
}
