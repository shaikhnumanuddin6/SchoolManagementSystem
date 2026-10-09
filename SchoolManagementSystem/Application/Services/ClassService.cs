using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Classes;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class ClassService : IClassService
{
    private readonly ApplicationDbContext _context;

    public ClassService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // CLASS LIST, SEARCH AND FILTERS
    // ============================================================

    public async Task<ClassListViewModel> GetClassesAsync(
        string? searchTerm = null,
        string? academicYear = null,
        string? status = null)
    {
        var query = _context.Classes
            .AsNoTracking()
            .AsQueryable();

        // Get all available academic years for the filter dropdown.
        var academicYears = await _context.Classes
            .AsNoTracking()
            .Select(c => c.AcademicYear)
            .Distinct()
            .OrderBy(y => y)
            .ToListAsync();

        // Search by class, section, academic year, teacher or room.
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();

            var hasRoomNumber =
                int.TryParse(searchTerm, out var roomNumber);

            query = query.Where(c =>
                c.ClassName.Contains(searchTerm) ||
                c.Section.Contains(searchTerm) ||
                c.AcademicYear.Contains(searchTerm) ||
                (c.Teacher != null &&
                    (
                        c.Teacher.FirstName.Contains(searchTerm) ||
                        c.Teacher.LastName.Contains(searchTerm) ||
                        c.Teacher.EmployeeNumber.Contains(searchTerm)
                    )) ||
                (hasRoomNumber && c.RoomNumber == roomNumber));
        }

        // Academic year filter.
        if (!string.IsNullOrWhiteSpace(academicYear))
        {
            academicYear = academicYear.Trim();

            query = query.Where(c =>
                c.AcademicYear == academicYear);
        }

        // Status filter.
        var normalizedStatus =
            status?.Trim().ToLowerInvariant();

        if (normalizedStatus == "active")
        {
            query = query.Where(c => c.IsActive);
        }
        else if (normalizedStatus == "inactive")
        {
            query = query.Where(c => !c.IsActive);
        }

        // Statistics represent the entire class collection,
        // not only the currently filtered results.
        var totalClasses =
            await _context.Classes.CountAsync();

        var activeClasses =
            await _context.Classes.CountAsync(c => c.IsActive);

        var inactiveClasses =
            await _context.Classes.CountAsync(c => !c.IsActive);

        // Load filtered class results dynamically.
        var classes = await query
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .ThenBy(c => c.AcademicYear)
            .Select(c => new ClassListItemViewModel
            {
                Id = c.Id,
                ClassName = c.ClassName,
                Section = c.Section,
                AcademicYear = c.AcademicYear,
                RoomNumber = c.RoomNumber,
                TeacherId = c.TeacherId,

                TeacherName = c.Teacher != null
                    ? c.Teacher.FirstName + " " + c.Teacher.LastName
                    : "Not Assigned",

                StudentCount = c.Students.Count(s => s.IsActive),
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new ClassListViewModel
        {
            Classes = classes,
            SearchTerm = searchTerm,
            AcademicYear = academicYear,
            Status = status,
            AcademicYears = academicYears,
            TotalClasses = totalClasses,
            ActiveClasses = activeClasses,
            InactiveClasses = inactiveClasses
        };
    }

    // ============================================================
    // CREATE VIEW MODEL
    // ============================================================

    public async Task<ClassCreateViewModel> GetCreateViewModelAsync()
    {
        var model = new ClassCreateViewModel
        {
            IsActive = true
        };

        model.Teachers = await GetTeacherOptionsAsync();

        return model;
    }

    // ============================================================
    // TEACHER DROPDOWN
    // ============================================================

    public async Task<List<SelectListItem>> GetTeacherOptionsAsync(
        int? selectedTeacherId = null)
    {
        var teachers = await _context.Teachers
            .AsNoTracking()
            .Where(t =>
                t.IsActive ||
                (selectedTeacherId.HasValue &&
                 t.Id == selectedTeacherId.Value))
            .OrderBy(t => t.FirstName)
            .ThenBy(t => t.LastName)
            .Select(t => new
            {
                t.Id,
                t.FirstName,
                t.LastName,
                t.EmployeeNumber,
                t.IsActive
            })
            .ToListAsync();

        var options = teachers
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),

                Text =
                    $"{t.FirstName} {t.LastName} " +
                    $"({t.EmployeeNumber})" +
                    (t.IsActive ? "" : " - Inactive"),

                Selected = t.Id == selectedTeacherId
            })
            .ToList();

        options.Insert(0, new SelectListItem
        {
            Value = "",
            Text = "-- Not Assigned --",
            Selected = !selectedTeacherId.HasValue
        });

        return options;
    }

    // ============================================================
    // CREATE CLASS
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage, int? ClassId)>
        CreateClassAsync(ClassCreateViewModel model)
    {
        var className = model.ClassName.Trim();
        var section = model.Section.Trim();
        var academicYear = model.AcademicYear.Trim();

        if (string.IsNullOrWhiteSpace(className) ||
            string.IsNullOrWhiteSpace(section) ||
            string.IsNullOrWhiteSpace(academicYear))
        {
            return (
                false,
                "Class name, section and academic year are required.",
                null);
        }

        // Prevent duplicate class combinations.
        var duplicateExists = await _context.Classes.AnyAsync(c =>
            c.ClassName == className &&
            c.Section == section &&
            c.AcademicYear == academicYear);

        if (duplicateExists)
        {
            return (
                false,
                "A class with the same class name, section and academic year already exists.",
                null);
        }

        // A new class can only be assigned to an active teacher.
        if (model.TeacherId.HasValue)
        {
            var teacherExists = await _context.Teachers.AnyAsync(t =>
                t.Id == model.TeacherId.Value &&
                t.IsActive);

            if (!teacherExists)
            {
                return (
                    false,
                    "The selected teacher does not exist or is inactive.",
                    null);
            }
        }

        var schoolClass = new SchoolClass
        {
            ClassName = className,
            Section = section,
            AcademicYear = academicYear,
            TeacherId = model.TeacherId,
            RoomNumber = model.RoomNumber,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Classes.Add(schoolClass);

        try
        {
            await _context.SaveChangesAsync();

            return (true, null, schoolClass.Id);
        }
        catch (DbUpdateException)
        {
            // The database has a unique index for class name,
            // section and academic year. It also protects against
            // duplicate records submitted concurrently.
            return (
                false,
                "Unable to save the class. Check for duplicate class details and verify the selected teacher.",
                null);
        }
    }

    // ============================================================
    // GET EDIT VIEW MODEL
    // ============================================================

    public async Task<ClassEditViewModel?> GetEditViewModelAsync(int id)
    {
        var schoolClass = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (schoolClass == null)
        {
            return null;
        }

        var model = new ClassEditViewModel
        {
            Id = schoolClass.Id,
            ClassName = schoolClass.ClassName,
            Section = schoolClass.Section,
            AcademicYear = schoolClass.AcademicYear,
            TeacherId = schoolClass.TeacherId,
            RoomNumber = schoolClass.RoomNumber,
            IsActive = schoolClass.IsActive
        };

        model.Teachers =
            await GetTeacherOptionsAsync(schoolClass.TeacherId);

        return model;
    }

    // ============================================================
    // UPDATE CLASS
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        UpdateClassAsync(ClassEditViewModel model)
    {
        var schoolClass = await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == model.Id);

        if (schoolClass == null)
        {
            return (false, "Class not found.");
        }

        var className = model.ClassName.Trim();
        var section = model.Section.Trim();
        var academicYear = model.AcademicYear.Trim();

        if (string.IsNullOrWhiteSpace(className) ||
            string.IsNullOrWhiteSpace(section) ||
            string.IsNullOrWhiteSpace(academicYear))
        {
            return (
                false,
                "Class name, section and academic year are required.");
        }

        var duplicateExists = await _context.Classes.AnyAsync(c =>
            c.Id != model.Id &&
            c.ClassName == className &&
            c.Section == section &&
            c.AcademicYear == academicYear);

        if (duplicateExists)
        {
            return (
                false,
                "Another class already uses this class name, section and academic year.");
        }

        // Allow an existing inactive teacher to remain assigned,
        // but don't allow a different inactive teacher to be assigned.
        if (model.TeacherId.HasValue)
        {
            var teacherExists = await _context.Teachers.AnyAsync(t =>
                t.Id == model.TeacherId.Value &&
                (t.IsActive || t.Id == schoolClass.TeacherId));

            if (!teacherExists)
            {
                return (
                    false,
                    "The selected teacher does not exist or cannot be assigned.");
            }
        }

        schoolClass.ClassName = className;
        schoolClass.Section = section;
        schoolClass.AcademicYear = academicYear;
        schoolClass.TeacherId = model.TeacherId;
        schoolClass.RoomNumber = model.RoomNumber;
        schoolClass.IsActive = model.IsActive;

        try
        {
            await _context.SaveChangesAsync();

            return (true, null);
        }
        catch (DbUpdateException)
        {
            return (
                false,
                "Unable to update the class. Check for duplicate details and verify the selected teacher.");
        }
    }

    // ============================================================
    // CLASS DETAILS AND STUDENTS
    // ============================================================

    public async Task<ClassDetailsViewModel?> GetDetailsAsync(int id)
    {
        var schoolClass = await _context.Classes
            .AsNoTracking()
            .Include(c => c.Teacher)
                .ThenInclude(t => t!.User)
            .Include(c => c.Students)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (schoolClass == null)
        {
            return null;
        }

        var students = schoolClass.Students
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Select(s => new ClassStudentViewModel
            {
                Id = s.Id,
                FullName = s.FirstName + " " + s.LastName,
                AdmissionNumber = s.AdmissionNumber,
                Email = s.User.Email,
                Status = s.Status,
                IsActive = s.IsActive
            })
            .ToList();

        return new ClassDetailsViewModel
        {
            Id = schoolClass.Id,
            ClassName = schoolClass.ClassName,
            Section = schoolClass.Section,
            AcademicYear = schoolClass.AcademicYear,
            RoomNumber = schoolClass.RoomNumber,
            TeacherId = schoolClass.TeacherId,

            TeacherName = schoolClass.Teacher == null
                ? "Not Assigned"
                : schoolClass.Teacher.FullName,

            TeacherEmail = schoolClass.Teacher?.User?.Email,

            StudentCount = students.Count(s => s.IsActive),
            IsActive = schoolClass.IsActive,
            CreatedAt = schoolClass.CreatedAt,
            Students = students
        };
    }

    // ============================================================
    // ARCHIVE CLASS
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        ArchiveClassAsync(int id)
    {
        var schoolClass = await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == id);

        if (schoolClass == null)
        {
            return (false, "Class not found.");
        }

        if (!schoolClass.IsActive)
        {
            return (true, null);
        }

        // Do not delete class, students, grades or attendance.
        schoolClass.IsActive = false;

        await _context.SaveChangesAsync();

        return (true, null);
    }

    // ============================================================
    // RESTORE CLASS
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        RestoreClassAsync(int id)
    {
        var schoolClass = await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == id);

        if (schoolClass == null)
        {
            return (false, "Class not found.");
        }

        if (schoolClass.IsActive)
        {
            return (true, null);
        }

        schoolClass.IsActive = true;

        await _context.SaveChangesAsync();

        return (true, null);
    }
}