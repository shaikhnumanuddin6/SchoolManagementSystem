using SchoolManagementSystem.Application.ViewModels;

namespace SchoolManagementSystem.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}

