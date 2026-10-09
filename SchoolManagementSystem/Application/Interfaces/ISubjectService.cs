using SchoolManagementSystem.Application.ViewModels.Subjects;

namespace SchoolManagementSystem.Application.Interfaces;

public interface ISubjectService
{
    Task<SubjectListViewModel> GetSubjectsAsync(
        string? searchTerm = null,
        string? status = null);

    Task<SubjectCreateViewModel> GetCreateViewModelAsync();

    Task<(bool Success, string? ErrorMessage, int? SubjectId)>
        CreateSubjectAsync(SubjectCreateViewModel model);

    Task<SubjectEditViewModel?> GetEditViewModelAsync(int id);

    Task<(bool Success, string? ErrorMessage)>
        UpdateSubjectAsync(SubjectEditViewModel model);

    Task<SubjectDetailsViewModel?> GetDetailsAsync(int id);

    Task<(bool Success, string? ErrorMessage)>
        ArchiveSubjectAsync(int id);

    Task<(bool Success, string? ErrorMessage)>
        RestoreSubjectAsync(int id);
}