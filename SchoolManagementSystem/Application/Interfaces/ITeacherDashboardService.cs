using SchoolManagementSystem.Application.ViewModels.Teachers;

namespace SchoolManagementSystem.Application.Interfaces;

public interface ITeacherDashboardService
{
    Task<TeacherDashboardViewModel?> GetDashboardAsync(
        string applicationUserId);
}