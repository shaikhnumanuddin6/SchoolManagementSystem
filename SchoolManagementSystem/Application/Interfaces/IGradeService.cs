using SchoolManagementSystem.Application.ViewModels.Grades;

namespace SchoolManagementSystem.Application.Interfaces;

public interface IGradeService
{
    Task<GradeListViewModel> GetAllAsync(
        GradeListViewModel? filter = null);

    Task<GradeDetailsViewModel?> GetDetailsAsync(int id);

    Task<GradeCreateViewModel> GetCreateViewModelAsync();

    Task<(bool Success, string? ErrorMessage)> CreateAsync(
        GradeCreateViewModel model);

    Task<GradeEditViewModel?> GetEditViewModelAsync(int id);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(
        GradeEditViewModel model);
}