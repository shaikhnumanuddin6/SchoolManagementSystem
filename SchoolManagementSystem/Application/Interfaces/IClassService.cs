using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagementSystem.Application.ViewModels.Classes;

namespace SchoolManagementSystem.Application.Interfaces;

public interface IClassService
{
    Task<ClassListViewModel> GetClassesAsync(
        string? searchTerm = null,
        string? academicYear = null,
        string? status = null);

    Task<ClassCreateViewModel> GetCreateViewModelAsync();

    Task<List<SelectListItem>> GetTeacherOptionsAsync(
        int? selectedTeacherId = null);

    Task<(bool Success, string? ErrorMessage, int? ClassId)>
        CreateClassAsync(ClassCreateViewModel model);

    Task<ClassEditViewModel?> GetEditViewModelAsync(int id);

    Task<(bool Success, string? ErrorMessage)>
        UpdateClassAsync(ClassEditViewModel model);

    Task<ClassDetailsViewModel?> GetDetailsAsync(int id);

    Task<(bool Success, string? ErrorMessage)>
        ArchiveClassAsync(int id);

    Task<(bool Success, string? ErrorMessage)>
        RestoreClassAsync(int id);
}