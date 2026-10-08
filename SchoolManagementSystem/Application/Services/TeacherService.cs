using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Teachers;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class TeacherService : ITeacherService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ============================================================
    // TEACHER LIST
    // ============================================================

    public async Task<TeacherListViewModel> GetTeachersAsync(
        string? searchTerm = null,
        string? status = null)
    {
        var query = _context.Teachers
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Classes)
            .AsQueryable();

        // --------------------------------------------------------
        // SEARCH
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();

            query = query.Where(t =>
                t.FirstName.Contains(searchTerm) ||
                t.LastName.Contains(searchTerm) ||
                t.EmployeeNumber.Contains(searchTerm) ||
                (t.User.Email != null &&
                 t.User.Email.Contains(searchTerm)) ||
                (t.Qualification != null &&
                 t.Qualification.Contains(searchTerm)) ||
                (t.SpecializationDepartment != null &&
                 t.SpecializationDepartment.Contains(searchTerm)));
        }

        // --------------------------------------------------------
        // STATUS FILTER
        // Active   = IsActive true
        // Inactive = IsActive false (or "archived")
        // --------------------------------------------------------

        string? selectedStatus = null;
        var normalizedStatus =
            status?.Trim().ToLowerInvariant();

        if (normalizedStatus == "active")
        {
            query = query.Where(t => t.IsActive);
            selectedStatus = "Active";
        }
        else if (normalizedStatus == "inactive" || normalizedStatus == "archived")
        {
            query = query.Where(t => !t.IsActive);
            selectedStatus = "Inactive";
        }
        else if (!string.IsNullOrWhiteSpace(status))
        {
            selectedStatus = status.Trim();
        }

        // --------------------------------------------------------
        // TEACHER LIST
        // --------------------------------------------------------

        var teachers = await query
            .OrderBy(t => t.FirstName)
            .ThenBy(t => t.LastName)
            .Select(t => new TeacherListItemViewModel
            {
                Id = t.Id,

                FullName =
                    t.FirstName + " " + t.LastName,

                EmployeeNumber =
                    t.EmployeeNumber,

                Email =
                    t.User.Email,

                Qualification =
                    t.Qualification,

                SpecializationDepartment =
                    t.SpecializationDepartment,

                Phone =
                    t.Phone,

                HireDate =
                    t.HireDate,

                IsActive =
                    t.IsActive,

                Status =
                    t.IsActive
                        ? "Active"
                        : "Archived",

                ClassCount =
                    t.Classes.Count
            })
            .ToListAsync();

        // --------------------------------------------------------
        // COUNTS
        // --------------------------------------------------------

        var totalTeachers =
            await _context.Teachers.CountAsync();

        var activeTeachers =
            await _context.Teachers
                .CountAsync(t => t.IsActive);

        var inactiveTeachers =
            await _context.Teachers
                .CountAsync(t => !t.IsActive);

        // --------------------------------------------------------
        // RETURN VIEW MODEL
        // --------------------------------------------------------

        return new TeacherListViewModel
        {
            Teachers =
                teachers,

            SearchTerm =
                searchTerm,

            Status =
                selectedStatus,

            TotalTeachers =
                totalTeachers,

            ActiveTeachers =
                activeTeachers,

            InactiveTeachers =
                inactiveTeachers
        };
    }

    // ============================================================
    // CREATE VIEW MODEL
    // ============================================================

    public Task<TeacherCreateViewModel>
        GetCreateViewModelAsync()
    {
        var model = new TeacherCreateViewModel
        {
            HireDate = DateTime.Today,
            IsActive = true
        };

        return Task.FromResult(model);
    }

    // ============================================================
    // CREATE TEACHER
    // ============================================================

    public async Task<(
        bool Success,
        string? ErrorMessage,
        int? TeacherId)>
        CreateTeacherAsync(
            TeacherCreateViewModel model)
    {
        var normalizedEmployeeNumber =
            model.EmployeeNumber
                .Trim()
                .ToUpperInvariant();

        var normalizedEmail =
            model.Email
                .Trim()
                .ToLowerInvariant();

        // --------------------------------------------------------
        // EMPLOYEE NUMBER CHECK
        // --------------------------------------------------------

        var employeeNumberExists =
            await _context.Teachers
                .AnyAsync(t =>
                    t.EmployeeNumber ==
                    normalizedEmployeeNumber);

        if (employeeNumberExists)
        {
            return (
                false,
                "A teacher with this employee number already exists.",
                null);
        }

        // --------------------------------------------------------
        // EMAIL CHECK
        // --------------------------------------------------------

        var existingUser =
            await _userManager.FindByEmailAsync(
                normalizedEmail);

        if (existingUser != null)
        {
            return (
                false,
                "An account with this email address already exists.",
                null);
        }

        // --------------------------------------------------------
        // EF CORE RETRY STRATEGY
        // --------------------------------------------------------

        var executionStrategy =
            _context.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction =
                    await _context.Database
                        .BeginTransactionAsync();

                try
                {
                    var firstName =
                        model.FirstName.Trim();

                    var lastName =
                        model.LastName.Trim();

                    // ------------------------------------------------
                    // CREATE IDENTITY USER
                    // ------------------------------------------------

                    var user = new ApplicationUser
                    {
                        UserName =
                            normalizedEmail,

                        Email =
                            normalizedEmail,

                        FirstName =
                            firstName,

                        LastName =
                            lastName,

                        IsActive =
                            model.IsActive,

                        CreatedAt =
                            DateTime.UtcNow
                    };

                    var userResult =
                        await _userManager.CreateAsync(
                            user,
                            model.Password);

                    if (!userResult.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        var errors = string.Join(
                            " ",
                            userResult.Errors.Select(
                                e => e.Description));

                        return (
                            false,
                            errors,
                            (int?)null);
                    }

                    // ------------------------------------------------
                    // ASSIGN TEACHER ROLE
                    // ------------------------------------------------

                    var roleResult =
                        await _userManager.AddToRoleAsync(
                            user,
                            "Teacher");

                    if (!roleResult.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        var errors = string.Join(
                            " ",
                            roleResult.Errors.Select(
                                e => e.Description));

                        return (
                            false,
                            $"Unable to assign Teacher role. {errors}",
                            (int?)null);
                    }

                    // ------------------------------------------------
                    // CREATE TEACHER
                    // ------------------------------------------------

                    var teacher = new Teacher
                    {
                        FirstName =
                            firstName,

                        LastName =
                            lastName,

                        EmployeeNumber =
                            normalizedEmployeeNumber,

                        Qualification =
                            string.IsNullOrWhiteSpace(
                                model.Qualification)
                                    ? null
                                    : model.Qualification.Trim(),

                        SpecializationDepartment =
                            string.IsNullOrWhiteSpace(
                                model.SpecializationDepartment)
                                    ? null
                                    : model.SpecializationDepartment.Trim(),

                        Phone =
                            string.IsNullOrWhiteSpace(
                                model.Phone)
                                    ? null
                                    : model.Phone.Trim(),

                        HireDate =
                            model.HireDate ??
                            DateTime.Today,

                        IsActive =
                            model.IsActive,

                        CreatedAt =
                            DateTime.UtcNow,

                        ApplicationUserId =
                            user.Id
                    };

                    _context.Teachers.Add(teacher);

                    // ------------------------------------------------
                    // SAVE
                    // ------------------------------------------------

                    await _context.SaveChangesAsync();

                    // ------------------------------------------------
                    // COMMIT
                    // ------------------------------------------------

                    await transaction.CommitAsync();

                    return (
                        true,
                        (string?)null,
                        (int?)teacher.Id);
                }
                catch (Exception ex)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch
                    {
                        // Preserve original error.
                    }

                    return (
                        false,
                        $"Unable to create teacher: {ex.Message}",
                        (int?)null);
                }
            });
    }

    // ============================================================
    // GET EDIT VIEW MODEL
    // ============================================================

    public async Task<TeacherEditViewModel?>
        GetEditViewModelAsync(int id)
    {
        var teacher =
            await _context.Teachers
                .AsNoTracking()
                .Include(t => t.User)
                .FirstOrDefaultAsync(
                    t => t.Id == id);

        if (teacher == null)
        {
            return null;
        }

        return new TeacherEditViewModel
        {
            Id =
                teacher.Id,

            FirstName =
                teacher.FirstName,

            LastName =
                teacher.LastName,

            EmployeeNumber =
                teacher.EmployeeNumber,

            Qualification =
                teacher.Qualification,

            SpecializationDepartment =
                teacher.SpecializationDepartment,

            Phone =
                teacher.Phone,

            HireDate =
                teacher.HireDate,

            Email =
                teacher.User.Email ??
                string.Empty,

            IsActive =
                teacher.IsActive,

            Status =
                teacher.IsActive
                    ? "Active"
                    : "Archived"
        };
    }

    // ============================================================
    // UPDATE TEACHER
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        UpdateTeacherAsync(
            TeacherEditViewModel model)
    {
        var teacher =
            await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(
                    t => t.Id == model.Id);

        if (teacher == null)
        {
            return (
                false,
                "Teacher not found.");
        }

        // --------------------------------------------------------
        // EMPLOYEE NUMBER CHECK
        // --------------------------------------------------------

        var normalizedEmployeeNumber =
            model.EmployeeNumber
                .Trim()
                .ToUpperInvariant();

        var employeeNumberExists =
            await _context.Teachers
                .AnyAsync(t =>
                    t.Id != model.Id &&
                    t.EmployeeNumber ==
                    normalizedEmployeeNumber);

        if (employeeNumberExists)
        {
            return (
                false,
                "Another teacher already uses this employee number.");
        }

        // --------------------------------------------------------
        // EMAIL CHECK
        // --------------------------------------------------------

        var normalizedEmail =
            model.Email
                .Trim()
                .ToLowerInvariant();

        var emailUser =
            await _userManager.FindByEmailAsync(
                normalizedEmail);

        if (emailUser != null &&
            emailUser.Id != teacher.ApplicationUserId)
        {
            return (
                false,
                "Another account already uses this email address.");
        }

        // --------------------------------------------------------
        // UPDATE TEACHER
        // --------------------------------------------------------

        teacher.FirstName =
            model.FirstName.Trim();

        teacher.LastName =
            model.LastName.Trim();

        teacher.EmployeeNumber =
            normalizedEmployeeNumber;

        teacher.Qualification =
            string.IsNullOrWhiteSpace(
                model.Qualification)
                    ? null
                    : model.Qualification.Trim();

        teacher.SpecializationDepartment =
            string.IsNullOrWhiteSpace(
                model.SpecializationDepartment)
                    ? null
                    : model.SpecializationDepartment.Trim();

        teacher.Phone =
            string.IsNullOrWhiteSpace(
                model.Phone)
                    ? null
                    : model.Phone.Trim();

        teacher.HireDate =
            model.HireDate ??
            teacher.HireDate;

        teacher.IsActive =
            model.IsActive;

        // --------------------------------------------------------
        // UPDATE IDENTITY USER
        // --------------------------------------------------------

        teacher.User.FirstName =
            teacher.FirstName;

        teacher.User.LastName =
            teacher.LastName;

        teacher.User.IsActive =
            teacher.IsActive;

        // --------------------------------------------------------
        // UPDATE EMAIL
        // --------------------------------------------------------

        var emailResult =
            await _userManager.SetEmailAsync(
                teacher.User,
                normalizedEmail);

        if (!emailResult.Succeeded)
        {
            var errors = string.Join(
                " ",
                emailResult.Errors.Select(
                    e => e.Description));

            return (
                false,
                $"Unable to update email: {errors}");
        }

        // --------------------------------------------------------
        // UPDATE USERNAME
        // --------------------------------------------------------

        var usernameResult =
            await _userManager.SetUserNameAsync(
                teacher.User,
                normalizedEmail);

        if (!usernameResult.Succeeded)
        {
            var errors = string.Join(
                " ",
                usernameResult.Errors.Select(
                    e => e.Description));

            return (
                false,
                $"Unable to update username: {errors}");
        }

        await _context.SaveChangesAsync();

        return (
            true,
            null);
    }

    // ============================================================
    // TEACHER DETAILS
    // ============================================================

    public async Task<TeacherDetailsViewModel?>
        GetDetailsAsync(int id)
    {
        var teacher =
            await _context.Teachers
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Classes)
                    .ThenInclude(c => c.Students)
                .FirstOrDefaultAsync(
                    t => t.Id == id);

        if (teacher == null)
        {
            return null;
        }

        var classes =
            teacher.Classes
                .OrderBy(c => c.ClassName)
                .ThenBy(c => c.Section)
                .Select(c => new TeacherClassViewModel
                {
                    Id =
                        c.Id,

                    ClassName =
                        c.ClassName,

                    Section =
                        c.Section,

                    AcademicYear =
                        c.AcademicYear,

                    RoomNumber =
                        c.RoomNumber,

                    StudentCount =
                        c.Students.Count,

                    IsActive =
                        c.IsActive
                })
                .ToList();

        return new TeacherDetailsViewModel
        {
            Id =
                teacher.Id,

            FullName =
                teacher.FullName,

            EmployeeNumber =
                teacher.EmployeeNumber,

            Email =
                teacher.User.Email,

            Qualification =
                teacher.Qualification,

            SpecializationDepartment =
                teacher.SpecializationDepartment,

            Phone =
                teacher.Phone,

            HireDate =
                teacher.HireDate,

            IsActive =
                teacher.IsActive,

            Status =
                teacher.IsActive
                    ? "Active"
                    : "Archived",

            ClassCount =
                classes.Count,

            Classes =
                classes
        };
    }

    // ============================================================
    // ARCHIVE TEACHER
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        ArchiveTeacherAsync(int id)
    {
        var teacher =
            await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(
                    t => t.Id == id);

        if (teacher == null)
        {
            return (
                false,
                "Teacher not found.");
        }

        teacher.IsActive =
            false;

        teacher.User.IsActive =
            false;

        await _context.SaveChangesAsync();

        return (
            true,
            null);
    }
}