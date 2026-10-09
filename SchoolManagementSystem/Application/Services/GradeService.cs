using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Grades;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class GradeService : IGradeService
{
    private readonly ApplicationDbContext _context;

    public GradeService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // LIST, SEARCH AND FILTER GRADES
    // ============================================================

    public async Task<GradeListViewModel> GetAllAsync(
        GradeListViewModel? filter = null)
    {
        filter ??= new GradeListViewModel();

        var query = _context.Grades
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();

            query = query.Where(g =>
                (g.Student.FirstName + " " +
                 g.Student.LastName).Contains(searchTerm) ||
                g.Student.AdmissionNumber.Contains(searchTerm) ||
                g.AssessmentName.Contains(searchTerm) ||
                g.SubjectName.Contains(searchTerm));
        }

        if (filter.ClassId.HasValue)
        {
            query = query.Where(g =>
                g.ClassId == filter.ClassId.Value);
        }

        if (filter.StudentId.HasValue)
        {
            query = query.Where(g =>
                g.StudentId == filter.StudentId.Value);
        }

        if (filter.SubjectId.HasValue)
        {
            query = query.Where(g =>
                g.SubjectId == filter.SubjectId.Value);
        }

        var grades = await query
            .OrderByDescending(g => g.ExamDate)
            .ThenBy(g => g.Student.LastName)
            .Select(g => new GradeListItemViewModel
            {
                Id = g.Id,

                StudentName =
                    g.Student.FirstName + " " +
                    g.Student.LastName,

                AdmissionNumber =
                    g.Student.AdmissionNumber,

                SubjectName =
                    g.Subject != null
                        ? g.Subject.SubjectName
                        : g.SubjectName,

                AssessmentName =
                    g.AssessmentName,

                MarksObtained =
                    g.MarksObtained,

                MaxMarks =
                    g.MaxMarks,

                Percentage =
                    g.MaxMarks <= 0
                        ? 0
                        : Math.Round(
                            (double)(
                                g.MarksObtained /
                                g.MaxMarks * 100m),
                            1),

                GradeLetter =
                    g.GradeLetter,

                ClassName =
                    g.Class == null
                        ? null
                        : g.Class.ClassName + " - " +
                          g.Class.Section,

                ExamDate =
                    g.ExamDate
            })
            .ToListAsync();

        var result = new GradeListViewModel
        {
            Grades = grades,
            SearchTerm = filter.SearchTerm,
            ClassId = filter.ClassId,
            StudentId = filter.StudentId,
            SubjectId = filter.SubjectId,

            TotalGrades = grades.Count,

            AveragePercentage = grades.Count == 0
                ? 0
                : Math.Round(
                    grades.Average(g => g.Percentage),
                    1)
        };

        await PopulateListLookupsAsync(result);

        return result;
    }


    // ============================================================
    // GRADE DETAILS
    // ============================================================

    public async Task<GradeDetailsViewModel?> GetDetailsAsync(
        int id)
    {
        return await _context.Grades
            .AsNoTracking()
            .Where(g => g.Id == id)
            .Select(g => new GradeDetailsViewModel
            {
                Id = g.Id,

                StudentName =
                    g.Student.FirstName + " " +
                    g.Student.LastName,

                AdmissionNumber =
                    g.Student.AdmissionNumber,

                SubjectName =
                    g.Subject != null
                        ? g.Subject.SubjectName
                        : g.SubjectName,

                AssessmentName =
                    g.AssessmentName,

                MarksObtained =
                    g.MarksObtained,

                MaxMarks =
                    g.MaxMarks,

                Percentage =
                    g.MaxMarks <= 0
                        ? 0
                        : Math.Round(
                            (double)(
                                g.MarksObtained /
                                g.MaxMarks * 100m),
                            1),

                GradeLetter =
                    g.GradeLetter,

                Feedback =
                    g.Feedback,

                ClassName =
                    g.Class == null
                        ? null
                        : g.Class.ClassName + " - " +
                          g.Class.Section,

                ExamDate =
                    g.ExamDate,

                CreatedAt =
                    g.CreatedAt
            })
            .FirstOrDefaultAsync();
    }


    // ============================================================
    // CREATE FORM
    // ============================================================

    public async Task<GradeCreateViewModel>
        GetCreateViewModelAsync()
    {
        var model = new GradeCreateViewModel();

        var lookups = await GetFormLookupsAsync(
            model.StudentId,
            model.ClassId,
            model.SubjectId);

        model.Students = lookups.Students;
        model.Classes = lookups.Classes;
        model.Subjects = lookups.Subjects;

        return model;
    }


    // ============================================================
    // CREATE GRADE
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        CreateAsync(GradeCreateViewModel model)
    {
        var marksError = ValidateMarks(
            model.MarksObtained,
            model.MaxMarks);

        if (marksError != null)
        {
            return (false, marksError);
        }

        var selection = await ValidateSelectionAsync(
            model.StudentId,
            model.ClassId,
            model.SubjectId);

        if (selection.ErrorMessage != null)
        {
            return (false, selection.ErrorMessage);
        }

        var student = selection.Student!;
        var schoolClass = selection.Class!;
        var subject = selection.Subject!;

        var assessmentName = model.AssessmentName.Trim();

        var duplicateExists =
            await DuplicateGradeExistsAsync(
                student.Id,
                subject.SubjectName,
                assessmentName);

        if (duplicateExists)
        {
            return (
                false,
                "A grade already exists for this student, " +
                "subject and assessment name.");
        }

        var grade = new Grade
        {
            StudentId = student.Id,
            ClassId = schoolClass.Id,

            SubjectId = subject.Id,
            SubjectName = subject.SubjectName,

            AssessmentName = assessmentName,
            MarksObtained = model.MarksObtained,
            MaxMarks = model.MaxMarks,

            Feedback = string.IsNullOrWhiteSpace(model.Feedback)
                ? null
                : model.Feedback.Trim(),

            ExamDate = model.ExamDate.Date,
            CreatedAt = DateTime.UtcNow
        };

        _context.Grades.Add(grade);

        await _context.SaveChangesAsync();

        return (true, null);
    }


    // ============================================================
    // EDIT FORM
    // ============================================================

    public async Task<GradeEditViewModel?> GetEditViewModelAsync(
        int id)
    {
        var grade = await _context.Grades
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grade == null)
        {
            return null;
        }

        // Support older grade records that have not yet been
        // linked to a SubjectId.
        var subjectId = grade.SubjectId;

        if (!subjectId.HasValue)
        {
            subjectId = await _context.Subjects
                .AsNoTracking()
                .Where(s => s.SubjectName == grade.SubjectName)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync();
        }

        // Use the student's current class if this older grade
        // does not have a ClassId.
        var studentClassId = await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == grade.StudentId)
            .Select(s => s.ClassId)
            .FirstOrDefaultAsync();

        var model = new GradeEditViewModel
        {
            Id = grade.Id,
            StudentId = grade.StudentId,
            ClassId = grade.ClassId ?? studentClassId,
            SubjectId = subjectId,

            AssessmentName = grade.AssessmentName,
            MarksObtained = grade.MarksObtained,
            MaxMarks = grade.MaxMarks,
            Feedback = grade.Feedback,
            ExamDate = grade.ExamDate
        };

        var lookups = await GetFormLookupsAsync(
            model.StudentId,
            model.ClassId,
            model.SubjectId);

        model.Students = lookups.Students;
        model.Classes = lookups.Classes;
        model.Subjects = lookups.Subjects;

        return model;
    }


    // ============================================================
    // UPDATE GRADE
    // ============================================================

    public async Task<(bool Success, string? ErrorMessage)>
        UpdateAsync(GradeEditViewModel model)
    {
        var grade = await _context.Grades
            .FirstOrDefaultAsync(g => g.Id == model.Id);

        if (grade == null)
        {
            return (false, "The grade could not be found.");
        }

        var marksError = ValidateMarks(
            model.MarksObtained,
            model.MaxMarks);

        if (marksError != null)
        {
            return (false, marksError);
        }

        var selection = await ValidateSelectionAsync(
            model.StudentId,
            model.ClassId,
            model.SubjectId);

        if (selection.ErrorMessage != null)
        {
            return (false, selection.ErrorMessage);
        }

        var student = selection.Student!;
        var schoolClass = selection.Class!;
        var subject = selection.Subject!;

        var assessmentName = model.AssessmentName.Trim();

        var duplicateExists =
            await DuplicateGradeExistsAsync(
                student.Id,
                subject.SubjectName,
                assessmentName,
                grade.Id);

        if (duplicateExists)
        {
            return (
                false,
                "A grade already exists for this student, " +
                "subject and assessment name.");
        }

        grade.StudentId = student.Id;
        grade.ClassId = schoolClass.Id;

        grade.SubjectId = subject.Id;
        grade.SubjectName = subject.SubjectName;

        grade.AssessmentName = assessmentName;
        grade.MarksObtained = model.MarksObtained;
        grade.MaxMarks = model.MaxMarks;

        grade.Feedback =
            string.IsNullOrWhiteSpace(model.Feedback)
                ? null
                : model.Feedback.Trim();

        grade.ExamDate = model.ExamDate.Date;

        await _context.SaveChangesAsync();

        return (true, null);
    }


    // ============================================================
    // VALIDATE MARKS
    // ============================================================

    private static string? ValidateMarks(
        decimal marksObtained,
        decimal maxMarks)
    {
        if (maxMarks <= 0)
        {
            return "Maximum marks must be greater than zero.";
        }

        if (marksObtained < 0)
        {
            return "Marks obtained cannot be negative.";
        }

        if (marksObtained > maxMarks)
        {
            return "Marks obtained cannot exceed maximum marks.";
        }

        return null;
    }


    // ============================================================
    // VALIDATE STUDENT, CLASS AND SUBJECT
    // ============================================================

    private async Task<(
        Student? Student,
        SchoolClass? Class,
        Subject? Subject,
        string? ErrorMessage)> ValidateSelectionAsync(
            int? studentId,
            int? classId,
            int? subjectId)
    {
        if (!studentId.HasValue ||
            !classId.HasValue ||
            !subjectId.HasValue)
        {
            return (
                null,
                null,
                null,
                "Please select a student, class and subject.");
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s =>
                s.Id == studentId.Value &&
                s.IsActive);

        if (student == null)
        {
            return (
                null,
                null,
                null,
                "The selected student is inactive or does not exist.");
        }

        var schoolClass = await _context.Classes
            .FirstOrDefaultAsync(c =>
                c.Id == classId.Value &&
                c.IsActive);

        if (schoolClass == null)
        {
            return (
                null,
                null,
                null,
                "The selected class is inactive or does not exist.");
        }

        if (student.ClassId != schoolClass.Id)
        {
            return (
                null,
                null,
                null,
                "The selected student does not belong to " +
                "the selected class.");
        }

        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s =>
                s.Id == subjectId.Value &&
                s.IsActive);

        if (subject == null)
        {
            return (
                null,
                null,
                null,
                "The selected subject is inactive or does not exist.");
        }

        return (
            student,
            schoolClass,
            subject,
            null);
    }


    // ============================================================
    // PREVENT DUPLICATE ASSESSMENTS
    // ============================================================

    private async Task<bool> DuplicateGradeExistsAsync(
        int studentId,
        string subjectName,
        string assessmentName,
        int? excludeGradeId = null)
    {
        var query = _context.Grades
            .AsNoTracking()
            .Where(g =>
                g.StudentId == studentId &&
                g.SubjectName == subjectName &&
                g.AssessmentName == assessmentName);

        if (excludeGradeId.HasValue)
        {
            query = query.Where(g =>
                g.Id != excludeGradeId.Value);
        }

        return await query.AnyAsync();
    }


    // ============================================================
    // FILTER DROPDOWNS
    // ============================================================

    private async Task PopulateListLookupsAsync(
        GradeListViewModel model)
    {
        var classes = await _context.Classes
            .AsNoTracking()
            .Where(c =>
                c.IsActive ||
                (model.ClassId.HasValue &&
                 c.Id == model.ClassId.Value))
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .ToListAsync();

        model.Classes = classes
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),

                Text =
                    c.ClassName + " - " +
                    c.Section + " (" +
                    c.AcademicYear + ")",

                Selected = model.ClassId == c.Id
            })
            .ToList();

        var students = await _context.Students
            .AsNoTracking()
            .Where(s =>
                s.IsActive ||
                (model.StudentId.HasValue &&
                 s.Id == model.StudentId.Value))
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        model.Students = students
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),

                Text =
                    s.LastName + ", " +
                    s.FirstName + " (" +
                    s.AdmissionNumber + ")",

                Selected = model.StudentId == s.Id
            })
            .ToList();

        var subjects = await _context.Subjects
            .AsNoTracking()
            .Where(s =>
                s.IsActive ||
                (model.SubjectId.HasValue &&
                 s.Id == model.SubjectId.Value))
            .OrderBy(s => s.SubjectName)
            .ToListAsync();

        model.Subjects = subjects
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),

                Text =
                    s.SubjectCode + " - " +
                    s.SubjectName,

                Selected = model.SubjectId == s.Id
            })
            .ToList();
    }


    // ============================================================
    // CREATE / EDIT FORM DROPDOWNS
    // ============================================================

    private async Task<(
        List<SelectListItem> Students,
        List<SelectListItem> Classes,
        List<SelectListItem> Subjects)> GetFormLookupsAsync(
            int? studentId,
            int? classId,
            int? subjectId)
    {
        var students = await _context.Students
            .AsNoTracking()
            .Where(s =>
                s.IsActive ||
                (studentId.HasValue &&
                 s.Id == studentId.Value))
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        var studentOptions = students
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),

                Text =
                    s.LastName + ", " +
                    s.FirstName + " (" +
                    s.AdmissionNumber + ")",

                Selected = studentId == s.Id
            })
            .ToList();

        var classes = await _context.Classes
            .AsNoTracking()
            .Where(c =>
                c.IsActive ||
                (classId.HasValue &&
                 c.Id == classId.Value))
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .ToListAsync();

        var classOptions = classes
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),

                Text =
                    c.ClassName + " - " +
                    c.Section + " (" +
                    c.AcademicYear + ")",

                Selected = classId == c.Id
            })
            .ToList();

        var subjects = await _context.Subjects
            .AsNoTracking()
            .Where(s =>
                s.IsActive ||
                (subjectId.HasValue &&
                 s.Id == subjectId.Value))
            .OrderBy(s => s.SubjectName)
            .ToListAsync();

        var subjectOptions = subjects
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),

                Text =
                    s.SubjectCode + " - " +
                    s.SubjectName,

                Selected = subjectId == s.Id
            })
            .ToList();

        return (
            studentOptions,
            classOptions,
            subjectOptions);
    }
}