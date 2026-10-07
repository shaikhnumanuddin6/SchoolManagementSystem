using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Students;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ============================================================
    // STUDENT LIST
    // ============================================================

    public async Task<StudentListViewModel> GetStudentsAsync(
        string? searchTerm = null,
        int? classId = null,
        string? status = null)
    {
        var query = _context.Students
            .AsNoTracking()
            .Include(s => s.Class)
            .Include(s => s.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();

            query = query.Where(s =>
                s.FirstName.Contains(searchTerm) ||
                s.LastName.Contains(searchTerm) ||
                s.AdmissionNumber.Contains(searchTerm) ||
                (s.User.Email != null &&
                 s.User.Email.Contains(searchTerm)));
        }

        if (classId.HasValue)
        {
            query = query.Where(
                s => s.ClassId == classId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(
                s => s.Status == status);
        }

        var students = await query
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Select(s => new StudentListItemViewModel
            {
                Id = s.Id,

                FullName =
                    s.FirstName + " " + s.LastName,

                AdmissionNumber =
                    s.AdmissionNumber,

                Email =
                    s.User.Email,

                ClassName =
                    s.Class != null
                        ? s.Class.ClassName
                        : null,

                Section =
                    s.Class != null
                        ? s.Class.Section
                        : null,

                Status =
                    s.Status,

                IsActive =
                    s.IsActive,

                EnrollmentDate =
                    s.EnrollmentDate
            })
            .ToListAsync();

        var classes = await _context.Classes
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .Select(c => new ClassFilterViewModel
            {
                Id = c.Id,

                DisplayName =
                    c.ClassName + " - " + c.Section
            })
            .ToListAsync();

        var totalStudents =
            await _context.Students.CountAsync();

        var activeStudents =
            await _context.Students
                .CountAsync(s => s.IsActive);

        return new StudentListViewModel
        {
            Students = students,

            SearchTerm = searchTerm,

            ClassId = classId,

            Status = status,

            Classes = classes,

            TotalStudents = totalStudents,

            ActiveStudents = activeStudents,

            InactiveStudents =
                totalStudents - activeStudents
        };
    }

    // ============================================================
    // CREATE VIEW MODEL
    // ============================================================

    public async Task<StudentCreateViewModel>
        GetCreateViewModelAsync()
    {
        return new StudentCreateViewModel
        {
            Classes =
                await GetClassFiltersAsync()
        };
    }

    // ============================================================
    // CREATE STUDENT
    // ============================================================

    public async Task<(
        bool Success,
        string? ErrorMessage,
        int? StudentId)>
        CreateStudentAsync(
            StudentCreateViewModel model)
    {
        var normalizedAdmissionNumber =
            model.AdmissionNumber
                .Trim()
                .ToUpperInvariant();

        var normalizedEmail =
            model.Email
                .Trim()
                .ToLowerInvariant();

        // --------------------------------------------------------
        // Validate admission number
        // --------------------------------------------------------

        var admissionExists =
            await _context.Students.AnyAsync(
                s =>
                    s.AdmissionNumber ==
                    normalizedAdmissionNumber);

        if (admissionExists)
        {
            return (
                false,
                "A student with this admission number already exists.",
                null);
        }

        // --------------------------------------------------------
        // Validate email
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
        // Execute entire transaction through EF Core retry strategy
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
                    // ------------------------------------------------
                    // Create Identity user
                    // ------------------------------------------------

                    var firstName =
                        model.FirstName.Trim();

                    var lastName =
                        model.LastName.Trim();

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
                            true,

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
                    // Assign Student role
                    // ------------------------------------------------

                    var roleResult =
                        await _userManager.AddToRoleAsync(
                            user,
                            "Student");

                    if (!roleResult.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        var errors = string.Join(
                            " ",
                            roleResult.Errors.Select(
                                e => e.Description));

                        return (
                            false,
                            $"Unable to assign Student role. {errors}",
                            (int?)null);
                    }

                    // ------------------------------------------------
                    // Create Student entity
                    // ------------------------------------------------

                    var student = new Student
                    {
                        FirstName =
                            firstName,

                        LastName =
                            lastName,

                        AdmissionNumber =
                            normalizedAdmissionNumber,

                        DateOfBirth =
                            model.DateOfBirth ??
                            DateTime.MinValue,

                        Gender =
                            string.IsNullOrWhiteSpace(
                                model.Gender)
                                    ? null
                                    : model.Gender.Trim(),

                        EmergencyContact =
                            string.IsNullOrWhiteSpace(
                                model.EmergencyContact)
                                    ? null
                                    : model.EmergencyContact.Trim(),

                        Address =
                            string.IsNullOrWhiteSpace(
                                model.Address)
                                    ? null
                                    : model.Address.Trim(),

                        EnrollmentDate =
                            DateTime.UtcNow,

                        Status =
                            string.IsNullOrWhiteSpace(
                                model.Status)
                                    ? "Active"
                                    : model.Status.Trim(),

                        IsActive =
                            true,

                        CreatedAt =
                            DateTime.UtcNow,

                        ApplicationUserId =
                            user.Id,

                        ClassId =
                            model.ClassId
                    };

                    _context.Students.Add(student);

                    // ------------------------------------------------
                    // Save student
                    // ------------------------------------------------

                    await _context.SaveChangesAsync();

                    // ------------------------------------------------
                    // Commit transaction
                    // ------------------------------------------------

                    await transaction.CommitAsync();

                    return (
                        true,
                        (string?)null,
                        (int?)student.Id);
                }
                catch (Exception ex)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch
                    {
                        // Do not hide the original exception.
                    }

                    return (
                        false,
                        $"Unable to create student: {ex.Message}",
                        (int?)null);
                }
            });
    }

    // ============================================================
    // GET EDIT VIEW MODEL
    // ============================================================

    public async Task<StudentEditViewModel?>
        GetEditViewModelAsync(int id)
    {
        var student =
            await _context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefaultAsync(
                    s => s.Id == id);

        if (student == null)
        {
            return null;
        }

        return new StudentEditViewModel
        {
            Id =
                student.Id,

            FirstName =
                student.FirstName,

            LastName =
                student.LastName,

            AdmissionNumber =
                student.AdmissionNumber,

            DateOfBirth =
                student.DateOfBirth,

            Gender =
                student.Gender,

            EmergencyContact =
                student.EmergencyContact,

            Address =
                student.Address,

            Email =
                student.User.Email ??
                string.Empty,

            ClassId =
                student.ClassId,

            Status =
                student.Status,

            IsActive =
                student.IsActive,

            Classes =
                await GetClassFiltersAsync()
        };
    }

    // ============================================================
    // UPDATE STUDENT
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        UpdateStudentAsync(
            StudentEditViewModel model)
    {
        var student =
            await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(
                    s => s.Id == model.Id);

        if (student == null)
        {
            return (
                false,
                "Student not found.");
        }

        var normalizedAdmissionNumber =
            model.AdmissionNumber
                .Trim()
                .ToUpperInvariant();

        var admissionExists =
            await _context.Students.AnyAsync(
                s =>
                    s.Id != model.Id &&
                    s.AdmissionNumber ==
                    normalizedAdmissionNumber);

        if (admissionExists)
        {
            return (
                false,
                "Another student already uses this admission number.");
        }

        var normalizedEmail =
            model.Email
                .Trim()
                .ToLowerInvariant();

        var emailUser =
            await _userManager.FindByEmailAsync(
                normalizedEmail);

        if (emailUser != null &&
            emailUser.Id != student.ApplicationUserId)
        {
            return (
                false,
                "Another account already uses this email address.");
        }

        // --------------------------------------------------------
        // Update Student information
        // --------------------------------------------------------

        student.FirstName =
            model.FirstName.Trim();

        student.LastName =
            model.LastName.Trim();

        student.AdmissionNumber =
            normalizedAdmissionNumber;

        student.DateOfBirth =
            model.DateOfBirth ??
            DateTime.MinValue;

        student.Gender =
            string.IsNullOrWhiteSpace(
                model.Gender)
                    ? null
                    : model.Gender.Trim();

        student.EmergencyContact =
            string.IsNullOrWhiteSpace(
                model.EmergencyContact)
                    ? null
                    : model.EmergencyContact.Trim();

        student.Address =
            string.IsNullOrWhiteSpace(
                model.Address)
                    ? null
                    : model.Address.Trim();

        student.ClassId =
            model.ClassId;

        student.Status =
            string.IsNullOrWhiteSpace(
                model.Status)
                    ? "Active"
                    : model.Status.Trim();

        student.IsActive =
            model.IsActive;

        // --------------------------------------------------------
        // Update Identity user
        // --------------------------------------------------------

        student.User.FirstName =
            student.FirstName;

        student.User.LastName =
            student.LastName;

        // Use UserManager so normalized Identity fields
        // are maintained correctly.
        var emailResult =
            await _userManager.SetEmailAsync(
                student.User,
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

        var usernameResult =
            await _userManager.SetUserNameAsync(
                student.User,
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
    // STUDENT DETAILS
    // ============================================================

    public async Task<StudentDetailsViewModel?>
        GetDetailsAsync(int id)
    {
        var student =
            await _context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Class)
                .Include(s => s.Attendances)
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(
                    s => s.Id == id);

        if (student == null)
        {
            return null;
        }

        // --------------------------------------------------------
        // Attendance
        // --------------------------------------------------------

        var attendanceCount =
            student.Attendances.Count;

        var presentCount =
            student.Attendances.Count(
                a => a.Status == AttendanceStatus.Present);

        var attendancePercentage =
            attendanceCount == 0
                ? 0
                : Math.Round(
                    (double)presentCount /
                    attendanceCount *
                    100,
                    1);

        // --------------------------------------------------------
        // Grades
        // --------------------------------------------------------

        var validGrades =
            student.Grades
                .Where(g => g.MaxMarks > 0)
                .ToList();

        var averageGradePercentage =
     validGrades.Count == 0
         ? 0
         : Math.Round(
             validGrades.Average(
                 g =>
                     (double)g.MarksObtained /
                     (double)g.MaxMarks *
                     100),
             1);

        // --------------------------------------------------------
        // Return details
        // --------------------------------------------------------

        return new StudentDetailsViewModel
        {
            Id =
                student.Id,

            FullName =
                student.FullName,

            AdmissionNumber =
                student.AdmissionNumber,

            Email =
                student.User.Email,

            DateOfBirth =
                student.DateOfBirth,

            Gender =
                student.Gender,

            EmergencyContact =
                student.EmergencyContact,

            Address =
                student.Address,

            EnrollmentDate =
                student.EnrollmentDate,

            Status =
                student.Status,

            IsActive =
                student.IsActive,

            ClassName =
                student.Class?.ClassName,

            Section =
                student.Class?.Section,

            AcademicYear =
                student.Class?.AcademicYear,

            AttendanceCount =
                attendanceCount,

            PresentCount =
                presentCount,

            AttendancePercentage =
                attendancePercentage,

            GradeCount =
                student.Grades.Count,

            AverageGradePercentage =
                averageGradePercentage
        };
    }

    // ============================================================
    // ARCHIVE STUDENT
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        ArchiveStudentAsync(int id)
    {
        var student =
            await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(
                    s => s.Id == id);

        if (student == null)
        {
            return (
                false,
                "Student not found.");
        }

        student.IsActive =
            false;

        student.Status =
            "Archived";

        student.User.IsActive =
            false;

        await _context.SaveChangesAsync();

        return (
            true,
            null);
    }

    // ============================================================
    // CLASS FILTERS
    // ============================================================

    private async Task<List<ClassFilterViewModel>>
        GetClassFiltersAsync()
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .Select(c => new ClassFilterViewModel
            {
                Id =
                    c.Id,

                DisplayName =
                    c.ClassName +
                    " - " +
                    c.Section
            })
            .ToListAsync();
    }
}