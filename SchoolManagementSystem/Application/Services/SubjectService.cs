using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Subjects;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class SubjectService : ISubjectService
{
    private readonly ApplicationDbContext _context;

    public SubjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // SUBJECT LIST, SEARCH AND FILTER
    // ============================================================

    public async Task<SubjectListViewModel> GetSubjectsAsync(
        string? searchTerm = null,
        string? status = null)
    {
        var query = _context.Subjects
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();

            query = query.Where(s =>
                s.SubjectCode.Contains(searchTerm) ||
                s.SubjectName.Contains(searchTerm) ||
                (s.Description != null &&
                 s.Description.Contains(searchTerm)));
        }

        var normalizedStatus =
            status?.Trim().ToLowerInvariant();

        if (normalizedStatus == "active")
        {
            query = query.Where(s => s.IsActive);
        }
        else if (normalizedStatus == "inactive" ||
                 normalizedStatus == "archived")
        {
            query = query.Where(s => !s.IsActive);
        }

        var totalSubjects =
            await _context.Subjects.CountAsync();

        var activeSubjects =
            await _context.Subjects.CountAsync(s => s.IsActive);

        var inactiveSubjects =
            await _context.Subjects.CountAsync(s => !s.IsActive);

        var subjects = await query
            .OrderBy(s => s.SubjectName)
            .Select(s => new SubjectListItemViewModel
            {
                Id = s.Id,
                SubjectCode = s.SubjectCode,
                SubjectName = s.SubjectName,
                Description = s.Description,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return new SubjectListViewModel
        {
            Subjects = subjects,
            SearchTerm = searchTerm,
            Status = status,
            TotalSubjects = totalSubjects,
            ActiveSubjects = activeSubjects,
            InactiveSubjects = inactiveSubjects
        };
    }

    // ============================================================
    // CREATE VIEW MODEL
    // ============================================================

    public Task<SubjectCreateViewModel> GetCreateViewModelAsync()
    {
        return Task.FromResult(
            new SubjectCreateViewModel
            {
                IsActive = true
            });
    }

    // ============================================================
    // CREATE SUBJECT
    // ============================================================

    public async Task<(
        bool Success,
        string? ErrorMessage,
        int? SubjectId)> CreateSubjectAsync(
            SubjectCreateViewModel model)
    {
        var normalizedCode =
            model.SubjectCode.Trim().ToUpperInvariant();

        var subjectName =
            model.SubjectName.Trim();

        var duplicateCode = await _context.Subjects
            .AnyAsync(s => s.SubjectCode == normalizedCode);

        if (duplicateCode)
        {
            return (
                false,
                "A subject with this code already exists.",
                null);
        }

        var subject = new Subject
        {
            SubjectCode = normalizedCode,
            SubjectName = subjectName,
            Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim(),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Subjects.Add(subject);

        try
        {
            await _context.SaveChangesAsync();

            return (true, null, subject.Id);
        }
        catch (DbUpdateException)
        {
            // The unique database index protects against duplicate
            // subject codes submitted concurrently.
            return (
                false,
                "Unable to save the subject. Check whether its code already exists.",
                null);
        }
    }

    // ============================================================
    // GET EDIT VIEW MODEL
    // ============================================================

    public async Task<SubjectEditViewModel?> GetEditViewModelAsync(int id)
    {
        var subject = await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return null;
        }

        return new SubjectEditViewModel
        {
            Id = subject.Id,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Description = subject.Description,
            IsActive = subject.IsActive
        };
    }

    // ============================================================
    // UPDATE SUBJECT
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        UpdateSubjectAsync(SubjectEditViewModel model)
    {
        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == model.Id);

        if (subject == null)
        {
            return (false, "Subject not found.");
        }

        var normalizedCode =
            model.SubjectCode.Trim().ToUpperInvariant();

        var subjectName =
            model.SubjectName.Trim();

        var duplicateCode = await _context.Subjects
            .AnyAsync(s =>
                s.Id != model.Id &&
                s.SubjectCode == normalizedCode);

        if (duplicateCode)
        {
            return (
                false,
                "Another subject already uses this subject code.");
        }

        subject.SubjectCode = normalizedCode;
        subject.SubjectName = subjectName;

        subject.Description =
            string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();

        subject.IsActive = model.IsActive;

        try
        {
            await _context.SaveChangesAsync();

            return (true, null);
        }
        catch (DbUpdateException)
        {
            return (
                false,
                "Unable to update the subject. Check for a duplicate subject code.");
        }
    }

    // ============================================================
    // SUBJECT DETAILS
    // ============================================================

    public async Task<SubjectDetailsViewModel?> GetDetailsAsync(int id)
    {
        var subject = await _context.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return null;
        }

        return new SubjectDetailsViewModel
        {
            Id = subject.Id,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Description = subject.Description,
            IsActive = subject.IsActive,
            CreatedAt = subject.CreatedAt
        };
    }

    // ============================================================
    // ARCHIVE SUBJECT
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        ArchiveSubjectAsync(int id)
    {
        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return (false, "Subject not found.");
        }

        if (!subject.IsActive)
        {
            return (true, null);
        }

        // Preserve the record instead of deleting it.
        subject.IsActive = false;

        await _context.SaveChangesAsync();

        return (true, null);
    }

    // ============================================================
    // RESTORE SUBJECT
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        RestoreSubjectAsync(int id)
    {
        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
        {
            return (false, "Subject not found.");
        }

        if (subject.IsActive)
        {
            return (true, null);
        }

        subject.IsActive = true;

        await _context.SaveChangesAsync();

        return (true, null);
    }
}