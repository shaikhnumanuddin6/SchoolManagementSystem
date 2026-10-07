using SchoolManagementSystem.Application.ViewModels.Teachers;

namespace SchoolManagementSystem.Application.Interfaces;

public interface ITeacherService
{
    // ============================================================
    // TEACHER LIST
    // ============================================================

    Task<TeacherListViewModel> GetTeachersAsync(
        string? searchTerm = null,
        string? status = null);


    // ============================================================
    // CREATE TEACHER
    // ============================================================

    Task<TeacherCreateViewModel>
        GetCreateViewModelAsync();

    Task<(
        bool Success,
        string? ErrorMessage,
        int? TeacherId)>
        CreateTeacherAsync(
            TeacherCreateViewModel model);


    // ============================================================
    // EDIT TEACHER
    // ============================================================

    Task<TeacherEditViewModel?>
        GetEditViewModelAsync(
            int id);

    Task<(
        bool Success,
        string? ErrorMessage)>
        UpdateTeacherAsync(
            TeacherEditViewModel model);


    // ============================================================
    // TEACHER DETAILS
    // ============================================================

    Task<TeacherDetailsViewModel?>
        GetDetailsAsync(
            int id);


    // ============================================================
    // ARCHIVE TEACHER
    // ============================================================

    Task<(
        bool Success,
        string? ErrorMessage)>
        ArchiveTeacherAsync(
            int id);
}