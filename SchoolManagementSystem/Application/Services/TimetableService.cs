
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Timetable;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class TimetableService : ITimetableService
{
    private readonly ApplicationDbContext _context;

    public TimetableService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TimetableListViewModel> GetListAsync(
        int? classId = null,
        int? dayOfWeek = null)
    {
        var model = new TimetableListViewModel
        {
            ClassId = classId,
            DayOfWeek = dayOfWeek,
            Classes = await GetClassOptionsAsync()
        };

        var query = _context.Timetables
            .AsNoTracking()
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .Include(t => t.Teacher)
            .AsQueryable();

        if (classId.HasValue)
        {
            query = query.Where(t => t.ClassId == classId.Value);
        }

        if (dayOfWeek.HasValue)
        {
            query = query.Where(t => t.DayOfWeek == dayOfWeek.Value);
        }

        var entries = await query
            .OrderBy(t => t.DayOfWeek)
            .ThenBy(t => t.StartTime)
            .ThenBy(t => t.Class.ClassName)
            .ToListAsync();

        model.Timetables = entries.Select(t => new TimetableItemViewModel
        {
            Id = t.Id,
            ClassId = t.ClassId,
            ClassName = string.IsNullOrWhiteSpace(t.Class.Section)
                ? t.Class.ClassName
                : $"{t.Class.ClassName} - {t.Class.Section}",
            SubjectName = t.Subject.SubjectName,
            TeacherName = t.Teacher.FullName,
            DayOfWeek = t.DayOfWeek,
            DayName = GetDayName(t.DayOfWeek),
            StartTime = t.StartTime,
            EndTime = t.EndTime,
            RoomNumber = t.RoomNumber
        }).ToList();

        return model;
    }

    public async Task<TimetableFormViewModel> GetFormViewModelAsync(
        int? id = null)
    {
        var model = new TimetableFormViewModel();

        if (id.HasValue)
        {
            var entry = await _context.Timetables
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id.Value);

            if (entry == null)
            {
                throw new KeyNotFoundException(
                    "The requested timetable entry was not found.");
            }

            model.Id = entry.Id;
            model.ClassId = entry.ClassId;
            model.SubjectId = entry.SubjectId;
            model.TeacherId = entry.TeacherId;
            model.DayOfWeek = entry.DayOfWeek;
            model.StartTime = entry.StartTime;
            model.EndTime = entry.EndTime;
            model.RoomNumber = entry.RoomNumber;
        }

        await PopulateDropdownsAsync(model);

        return model;
    }

    public async Task<(bool Success, string? ErrorMessage)> SaveAsync(
        TimetableFormViewModel model)
    {
        if (model.DayOfWeek < 1 || model.DayOfWeek > 7)
        {
            return (false, "Please select a valid day.");
        }

        if (model.StartTime < TimeSpan.Zero ||
            model.StartTime >= TimeSpan.FromDays(1) ||
            model.EndTime <= TimeSpan.Zero ||
            model.EndTime > TimeSpan.FromDays(1) ||
            model.EndTime <= model.StartTime)
        {
            return (false,
                "The end time must be after the start time, and both times must be valid.");
        }

        var schoolClass = await _context.Classes
            .AnyAsync(c => c.Id == model.ClassId && c.IsActive);

        if (!schoolClass)
        {
            return (false, "Please select an active class.");
        }

        var subjectExists = await _context.Subjects
            .AnyAsync(s => s.Id == model.SubjectId && s.IsActive);

        if (!subjectExists)
        {
            return (false, "Please select an active subject.");
        }

        var teacherExists = await _context.Teachers
            .AnyAsync(t => t.Id == model.TeacherId && t.IsActive);

        if (!teacherExists)
        {
            return (false, "Please select an active teacher.");
        }

        if (!string.IsNullOrWhiteSpace(model.RoomNumber) &&
            model.RoomNumber.Trim().Length > 50)
        {
            return (false, "Room number cannot exceed 50 characters.");
        }

        Timetable? entry = null;

        if (model.Id > 0)
        {
            entry = await _context.Timetables
                .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (entry == null)
            {
                return (false, "The timetable entry was not found.");
            }
        }

        // A lesson overlaps when its start is before the existing
        // lesson ends and its end is after the existing lesson starts.
        var conflictQuery = _context.Timetables
            .Where(t =>
                t.DayOfWeek == model.DayOfWeek &&
                t.StartTime < model.EndTime &&
                t.EndTime > model.StartTime);

        if (model.Id > 0)
        {
            conflictQuery = conflictQuery.Where(t => t.Id != model.Id);
        }

        var classConflict = await conflictQuery
            .AnyAsync(t => t.ClassId == model.ClassId);

        if (classConflict)
        {
            return (false,
                "This class already has a lesson scheduled during that time.");
        }

        var teacherConflict = await conflictQuery
            .AnyAsync(t => t.TeacherId == model.TeacherId);

        if (teacherConflict)
        {
            return (false,
                "This teacher is already assigned to another lesson during that time.");
        }

        if (entry == null)
        {
            entry = new Timetable
            {
                CreatedAt = DateTime.UtcNow
            };

            _context.Timetables.Add(entry);
        }

        entry.ClassId = model.ClassId;
        entry.SubjectId = model.SubjectId;
        entry.TeacherId = model.TeacherId;
        entry.DayOfWeek = model.DayOfWeek;
        entry.StartTime = model.StartTime;
        entry.EndTime = model.EndTime;
        entry.RoomNumber = string.IsNullOrWhiteSpace(model.RoomNumber)
            ? null
            : model.RoomNumber.Trim();

        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _context.Timetables
            .FirstOrDefaultAsync(t => t.Id == id);

        if (entry == null)
        {
            return false;
        }

        _context.Timetables.Remove(entry);

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task PopulateDropdownsAsync(
        TimetableFormViewModel model)
    {
        model.Classes = await GetClassOptionsAsync();

        model.Subjects = await _context.Subjects
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.SubjectName)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.SubjectName
            })
            .ToListAsync();

        model.Teachers = await _context.Teachers
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.FirstName)
            .ThenBy(t => t.LastName)
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.FirstName + " " + t.LastName
            })
            .ToListAsync();
    }

    private async Task<List<SelectListItem>> GetClassOptionsAsync()
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(c.Section)
                    ? c.ClassName
                    : c.ClassName + " - " + c.Section
            })
            .ToListAsync();
    }

    private static string GetDayName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            1 => "Monday",
            2 => "Tuesday",
            3 => "Wednesday",
            4 => "Thursday",
            5 => "Friday",
            6 => "Saturday",
            7 => "Sunday",
            _ => "Unknown"
        };
    }
}
