using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Teachers;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class TeacherDashboardService : ITeacherDashboardService
{
    private readonly ApplicationDbContext _context;

    public TeacherDashboardService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherDashboardViewModel?>
        GetDashboardAsync(string applicationUserId)
    {
        // ========================================================
        // FIND TEACHER
        // ========================================================

        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.ApplicationUserId == applicationUserId);

        if (teacher == null)
        {
            return null;
        }


        // ========================================================
        // TEACHER'S ACTIVE CLASSES
        // ========================================================

        var classes = await _context.Classes
            .AsNoTracking()
            .Where(c =>
                c.TeacherId == teacher.Id &&
                c.IsActive)
            .Include(c => c.Students)
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .ToListAsync();

        var classIds = classes
            .Select(c => c.Id)
            .ToList();


        // ========================================================
        // TOTAL STUDENTS
        // ========================================================

        var totalStudents = classes
            .SelectMany(c => c.Students)
            .Count(s => s.IsActive);


        // ========================================================
        // TODAY
        // ========================================================

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);


        // ========================================================
        // TODAY'S ATTENDANCE
        // ========================================================

        var todayAttendance = classIds.Count == 0
            ? new List<TodayAttendanceRecord>()
            : await _context.Attendances
                .AsNoTracking()
                .Where(a =>
                    a.ClassId.HasValue &&
                    classIds.Contains(a.ClassId.Value) &&
                    a.Date >= today &&
                    a.Date < tomorrow)
                .Select(a => new TodayAttendanceRecord
                {
                    Status = a.Status
                })
                .ToListAsync();


        var presentToday =
            todayAttendance.Count(a =>
                a.Status == "Present");

        var absentToday =
            todayAttendance.Count(a =>
                a.Status == "Absent");

        var lateToday =
            todayAttendance.Count(a =>
                a.Status == "Late");

        var attendanceRecordsToday =
            presentToday +
            absentToday +
            lateToday;

        var attendancePercentage =
            attendanceRecordsToday == 0
                ? 0
                : Math.Round(
                    (double)presentToday /
                    attendanceRecordsToday *
                    100,
                    1);


        // ========================================================
        // CLASS-WISE ATTENDANCE
        // ========================================================

        var classAttendanceRecords = classIds.Count == 0
            ? new List<ClassAttendanceRecord>()
            : await _context.Attendances
                .AsNoTracking()
                .Where(a =>
                    a.ClassId.HasValue &&
                    classIds.Contains(a.ClassId.Value) &&
                    a.Date >= today &&
                    a.Date < tomorrow)
                .Select(a => new ClassAttendanceRecord
                {
                    ClassId = a.ClassId!.Value,
                    Status = a.Status
                })
                .ToListAsync();


        var classDashboard =
            classes.Select(c =>
            {
                var records = classAttendanceRecords
                    .Where(a => a.ClassId == c.Id)
                    .ToList();

                var present =
                    records.Count(a => a.Status == "Present");

                var absent =
                    records.Count(a => a.Status == "Absent");

                var late =
                    records.Count(a => a.Status == "Late");

                var totalRecorded =
                    present +
                    absent +
                    late;

                var percentage =
                    totalRecorded == 0
                        ? 0
                        : Math.Round(
                            (double)present /
                            totalRecorded *
                            100,
                            1);

                return new TeacherClassDashboardViewModel
                {
                    ClassId = c.Id,
                    ClassName = c.ClassName,
                    Section = c.Section,
                    AcademicYear = c.AcademicYear,
                    StudentCount =
                        c.Students.Count(s => s.IsActive),
                    PresentCount = present,
                    AbsentCount = absent,
                    LateCount = late,
                    AttendancePercentage =
                        percentage
                };
            })
            .ToList();


        // ========================================================
        // RECENT ATTENDANCE
        // ========================================================

        var recentAttendance = classIds.Count == 0
            ? new List<TeacherAttendanceViewModel>()
            : await _context.Attendances
                .AsNoTracking()
                .Where(a =>
                    a.ClassId.HasValue &&
                    classIds.Contains(a.ClassId.Value))
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.CreatedAt)
                .Take(20)
                .Select(a => new TeacherAttendanceViewModel
                {
                    Id = a.Id,

                    StudentName =
                        a.Student.FirstName +
                        " " +
                        a.Student.LastName,

                    ClassName =
                        a.Class != null
                            ? a.Class.ClassName
                            : "Unassigned",

                    Section =
                        a.Class != null
                            ? a.Class.Section
                            : "-",

                    Status = a.Status,

                    Date = a.Date,

                    Remarks = a.Remarks
                })
                .ToListAsync();


        // ========================================================
        // GRADES
        // ========================================================

        var grades = classIds.Count == 0
            ? new List<GradeRecord>()
            : await _context.Grades
                .AsNoTracking()
                .Where(g =>
                    g.ClassId.HasValue &&
                    classIds.Contains(g.ClassId.Value) &&
                    g.MaxMarks > 0)
                .Select(g => new GradeRecord
                {
                    Id = g.Id,

                    StudentName =
                        g.Student.FirstName +
                        " " +
                        g.Student.LastName,

                    ClassName =
                        g.Class != null
                            ? g.Class.ClassName
                            : "Unassigned",

                    Section =
                        g.Class != null
                            ? g.Class.Section
                            : "-",

                    SubjectName = g.SubjectName,

                    AssessmentName =
                        g.AssessmentName,

                    MarksObtained =
                        g.MarksObtained,

                    MaxMarks =
                        g.MaxMarks,

                    ExamDate =
                        g.ExamDate
                })
                .ToListAsync();


        var totalGrades = grades.Count;

        var averageGradePercentage =
            totalGrades == 0
                ? 0
                : Math.Round(
                    grades
                        .Select(g =>
                            (double)g.MarksObtained /
                            (double)g.MaxMarks *
                            100)
                        .Average(),
                    1);


        // ========================================================
        // RECENT GRADES
        // ========================================================

        var recentGrades = grades
            .OrderByDescending(g => g.ExamDate)
            .Take(15)
            .Select(g => new TeacherGradeViewModel
            {
                Id = g.Id,

                StudentName =
                    g.StudentName,

                ClassName =
                    g.ClassName,

                Section =
                    g.Section,

                SubjectName =
                    g.SubjectName,

                AssessmentName =
                    g.AssessmentName,

                MarksObtained =
                    g.MarksObtained,

                MaxMarks =
                    g.MaxMarks,

                Percentage =
                    Math.Round(
                        (double)g.MarksObtained /
                        (double)g.MaxMarks *
                        100,
                        1),

                ExamDate =
                    g.ExamDate
            })
            .ToList();


        // ========================================================
        // PERFORMANCE BY SUBJECT
        // ========================================================

        var performanceData = grades
            .GroupBy(g => g.SubjectName)
            .Select(group => new TeacherPerformanceViewModel
            {
                SubjectName = group.Key,

                Percentage =
                    Math.Round(
                        group
                            .Select(g =>
                                (double)g.MarksObtained /
                                (double)g.MaxMarks *
                                100)
                            .Average(),
                        1)
            })
            .OrderByDescending(x => x.Percentage)
            .ToList();


        // ========================================================
        // RETURN
        // ========================================================

        return new TeacherDashboardViewModel
        {
            TeacherName =
                $"{teacher.FirstName} {teacher.LastName}".Trim(),

            AssignedClasses =
                classes.Count,

            TotalStudents =
                totalStudents,

            AttendancePercentage =
                attendancePercentage,

            PresentToday =
                presentToday,

            AbsentToday =
                absentToday,

            LateToday =
                lateToday,

            AttendanceRecordsToday =
                attendanceRecordsToday,

            TotalGrades =
                totalGrades,

            AverageGradePercentage =
                averageGradePercentage,

            Classes =
                classDashboard,

            RecentAttendance =
                recentAttendance,

            RecentGrades =
                recentGrades,

            PerformanceData =
                performanceData
        };
    }


    // ============================================================
    // INTERNAL DATA TYPES
    // ============================================================

    private sealed class TodayAttendanceRecord
    {
        public string Status { get; set; } = string.Empty;
    }


    private sealed class ClassAttendanceRecord
    {
        public int ClassId { get; set; }

        public string Status { get; set; } = string.Empty;
    }


    private sealed class GradeRecord
    {
        public int Id { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public string Section { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public string AssessmentName { get; set; } = string.Empty;

        public decimal MarksObtained { get; set; }

        public decimal MaxMarks { get; set; }

        public DateTime ExamDate { get; set; }
    }
}