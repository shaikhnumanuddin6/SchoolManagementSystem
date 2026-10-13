
using SchoolManagementSystem.Application.ViewModels.Timetable;

namespace SchoolManagementSystem.Application.Interfaces;

public interface ITimetableService
{
    Task<TimetableListViewModel> GetListAsync(
        int? classId = null,
        int? dayOfWeek = null);

    Task<TimetableFormViewModel> GetFormViewModelAsync(
        int? id = null);

    Task<(bool Success, string? ErrorMessage)> SaveAsync(
        TimetableFormViewModel model);

    Task<bool> DeleteAsync(int id);
}
