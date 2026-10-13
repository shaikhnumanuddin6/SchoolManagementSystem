
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Attendance;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _context;

    private static readonly string[] AllowedStatuses =
    {
        "Present",
        "Absent",
        "Late",
        "Excused"
    };

    public AttendanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceListViewModel> GetListAsync(
        AttendanceListViewModel? filter = null)
    {
        filter ??= new AttendanceListViewModel();

        var selectedDate = filter.Date.Date;

        var model = new AttendanceListViewModel
        {
            Date = selectedDate,
            ClassId = filter.ClassId,
            StudentId = filter.StudentId,
            Status = filter.Status
        };

        model.Classes = await _context.Classes
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.ClassName + " - " + c.Section
                    + " (" + c.AcademicYear + ")"
            })
            .ToListAsync();

        model.Students = await _context.Students
            .AsNoTracking()
            .Where(s => s.IsActive &&
                (!model.ClassId.HasValue ||
                 s.ClassId == model.ClassId.Value))
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.FirstName + " " + s.LastName
                    + " (" + s.AdmissionNumber + ")"
            })
            .ToListAsync();

        model.Statuses = AllowedStatuses
            .Select(status => new SelectListItem
            {
                Value = status,
                Text = status,
                Selected = status == model.Status
            })
            .ToList();

        var query = _context.Attendances
            .AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => a.Date >= selectedDate &&
                        a.Date < selectedDate.AddDays(1));

        if (model.ClassId.HasValue)
        {
            query = query.Where(a =>
                a.ClassId == model.ClassId.Value);
        }

        if (model.StudentId.HasValue)
        {
            query = query.Where(a =>
                a.StudentId == model.StudentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(model.Status))
        {
            if (!AllowedStatuses.Contains(
                    model.Status,
                    StringComparer.OrdinalIgnoreCase))
            {
                model.Status = null;
            }
            else
            {
                var selectedStatus = model.Status;

                query = query.Where(a =>
                    a.Status == selectedStatus);
            }
        }

        model.Records = await query
            .OrderBy(a => a.Student.FirstName)
            .ThenBy(a => a.Student.LastName)
            .Select(a => new AttendanceListItemViewModel
            {
                Id = a.Id,
                StudentId = a.StudentId,
                StudentName = a.Student.FirstName + " "
                    + a.Student.LastName,
                AdmissionNumber = a.Student.AdmissionNumber,
                ClassId = a.ClassId,
                ClassName = a.Class == null
                    ? "Not assigned"
                    : a.Class.ClassName + " - "
                        + a.Class.Section,
                Date = a.Date,
                Status = a.Status,
                Remarks = a.Remarks
            })
            .ToListAsync();

        model.TotalRecords = model.Records.Count;
        model.PresentCount = model.Records.Count(
            r => r.Status == "Present");
        model.AbsentCount = model.Records.Count(
            r => r.Status == "Absent");
        model.LateCount = model.Records.Count(
            r => r.Status == "Late");
        model.ExcusedCount = model.Records.Count(
            r => r.Status == "Excused");

        // Excused students are excluded from the attendance
        // percentage denominator.
        var eligibleRecords = model.TotalRecords
            - model.ExcusedCount;

        model.AttendancePercentage = eligibleRecords == 0
            ? 0
            : Math.Round(
                (double)(model.PresentCount + model.LateCount)
                / eligibleRecords * 100,
                2);

        return model;
    }

    public async Task<AttendanceMarkViewModel>
        GetMarkViewModelAsync(int? classId, DateTime? date)
    {
        var selectedDate = (date ?? DateTime.Today).Date;

        var model = new AttendanceMarkViewModel
        {
            ClassId = classId,
            Date = selectedDate
        };

        model.Classes = await _context.Classes
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.ClassName + " - " + c.Section
                    + " (" + c.AcademicYear + ")"
            })
            .ToListAsync();

        if (!classId.HasValue)
        {
            return model;
        }

        var selectedClassExists = await _context.Classes
            .AnyAsync(c =>
                c.Id == classId.Value && c.IsActive);

        if (!selectedClassExists)
        {
            model.ClassId = null;
            return model;
        }

        var students = await _context.Students
            .AsNoTracking()
            .Where(s =>
                s.IsActive &&
                s.ClassId == classId.Value)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Select(s => new
            {
                s.Id,
                s.FirstName,
                s.LastName,
                s.AdmissionNumber
            })
            .ToListAsync();

        var existingRecords = await _context.Attendances
            .AsNoTracking()
            .Where(a =>
                a.ClassId == classId.Value &&
                a.Date >= selectedDate &&
                a.Date < selectedDate.AddDays(1))
            .ToDictionaryAsync(a => a.StudentId);

        model.Students = students
            .Select(s =>
            {
                existingRecords.TryGetValue(
                    s.Id, out var existing);

                return new AttendanceStudentEntryViewModel
                {
                    StudentId = s.Id,
                    StudentName = s.FirstName + " " + s.LastName,
                    AdmissionNumber = s.AdmissionNumber,
                    Status = existing?.Status ?? "Present",
                    Remarks = existing?.Remarks
                };
            })
            .ToList();

        return model;
    }

    public async Task<(bool Success, string? ErrorMessage)>
        SaveAttendanceAsync(AttendanceMarkViewModel model)
    {
        if (!model.ClassId.HasValue)
        {
            return (false, "Please select a class.");
        }

        if (model.Date == default)
        {
            return (false, "Please select a valid attendance date.");
        }

        var selectedDate = model.Date.Date;
        var classId = model.ClassId.Value;

        var selectedClass = await _context.Classes
            .FirstOrDefaultAsync(c =>
                c.Id == classId && c.IsActive);

        if (selectedClass == null)
        {
            return (false,
                "The selected class does not exist or is inactive.");
        }

        if (model.Students == null || model.Students.Count == 0)
        {
            return (false,
                "There are no students to save attendance for.");
        }

        var duplicateStudentIds = model.Students
            .GroupBy(s => s.StudentId)
            .Any(g => g.Count() > 1);

        if (duplicateStudentIds)
        {
            return (false,
                "The attendance form contains duplicate students.");
        }

        foreach (var student in model.Students)
        {
            if (!AllowedStatuses.Contains(
                    student.Status ?? string.Empty,
                    StringComparer.Ordinal))
            {
                return (false,
                    $"Invalid attendance status for student ID {student.StudentId}.");
            }

            if (student.Remarks?.Length > 500)
            {
                return (false,
                    "Attendance remarks cannot exceed 500 characters.");
            }
        }

        var submittedStudentIds = model.Students
            .Select(s => s.StudentId)
            .ToList();

        var validStudentIds = await _context.Students
            .Where(s =>
                s.IsActive &&
                s.ClassId == classId &&
                submittedStudentIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        if (validStudentIds.Count != submittedStudentIds.Count)
        {
            return (false,
                "One or more students are inactive, do not belong to the selected class, or no longer exist. Refresh the register and try again.");
        }

        var existingRecords = await _context.Attendances
            .Where(a =>
                submittedStudentIds.Contains(a.StudentId) &&
                a.Date >= selectedDate &&
                a.Date < selectedDate.AddDays(1))
            .ToListAsync();

        var recordsByStudent = existingRecords
            .ToDictionary(a => a.StudentId);

        foreach (var student in model.Students)
        {
            if (recordsByStudent.TryGetValue(
                    student.StudentId, out var attendance))
            {
                // Update the existing daily record.
                attendance.Status = student.Status;
                attendance.Remarks = student.Remarks;
                attendance.ClassId = classId;
            }
            else
            {
                // Create a record only if one does not exist.
                _context.Attendances.Add(new Attendance
                {
                    StudentId = student.StudentId,
                    ClassId = classId,
                    Date = selectedDate,
                    Status = student.Status,
                    Remarks = student.Remarks,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return (false,
                "Attendance could not be saved. A database conflict may have occurred. Refresh the register and try again.");
        }

        return (true, null);
    }
}
