using FastFood.MVC.ViewModels;

namespace FastFood.MVC.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
}
