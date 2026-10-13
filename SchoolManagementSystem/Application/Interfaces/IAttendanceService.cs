
using SchoolManagementSystem.Application.ViewModels.Attendance;

namespace SchoolManagementSystem.Application.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceListViewModel> GetListAsync(
        AttendanceListViewModel? filter = null);

    Task<AttendanceMarkViewModel> GetMarkViewModelAsync(
        int? classId,
        DateTime? date);

    Task<(bool Success, string? ErrorMessage)> SaveAttendanceAsync(
        AttendanceMarkViewModel model);
}
