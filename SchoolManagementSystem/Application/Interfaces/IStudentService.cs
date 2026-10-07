
using SchoolManagementSystem.Application.ViewModels.Students;

namespace SchoolManagementSystem.Application.Interfaces;

public interface IStudentService
{
    Task<StudentListViewModel> GetStudentsAsync(
        string? searchTerm = null,
        int? classId = null,
        string? status = null);

    Task<StudentCreateViewModel> GetCreateViewModelAsync();

    Task<(bool Success, string? ErrorMessage, int? StudentId)>
        CreateStudentAsync(StudentCreateViewModel model);

    Task<StudentEditViewModel?> GetEditViewModelAsync(int id);

    Task<(bool Success, string? ErrorMessage)>
        UpdateStudentAsync(StudentEditViewModel model);

    Task<StudentDetailsViewModel?> GetDetailsAsync(int id);

    Task<(bool Success, string? ErrorMessage)>
        ArchiveStudentAsync(int id);
}

