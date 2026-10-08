using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // ========================================================
        // BASIC COUNTS
        // ========================================================

        var totalStudents =
            await _context.Students.CountAsync();

        var totalTeachers =
            await _context.Teachers.CountAsync();

        var totalClasses =
            await _context.Classes.CountAsync();


        // ========================================================
        // TODAY'S ATTENDANCE
        // ========================================================

        var todayAttendance = await _context.Attendances
            .AsNoTracking()
            .Where(a =>
                a.Date >= today &&
                a.Date < tomorrow)
            .Select(a => new
            {
                a.Id,
                a.Date,
                a.Status,
                a.Remarks,
                StudentName =
                    a.Student.FirstName + " " + a.Student.LastName,
                ClassName =
                    a.Class != null
                        ? a.Class.ClassName
                        : "Unassigned",
                Section =
                    a.Class != null
                        ? a.Class.Section
                        : "-"
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
                    (double)presentToday
                    / attendanceRecordsToday
                    * 100,
                    1);


        // ========================================================
        // CLASS-WISE ATTENDANCE
        // ========================================================

        var classes = await _context.Classes
            .AsNoTracking()
            .Include(c => c.Teacher)
            .Include(c => c.Students)
            .OrderBy(c => c.ClassName)
            .ThenBy(c => c.Section)
            .ToListAsync();

        var classIds = classes
            .Select(c => c.Id)
            .ToList();

        var classAttendance = await _context.Attendances
            .AsNoTracking()
            .Where(a =>
                a.ClassId.HasValue &&
                classIds.Contains(a.ClassId.Value) &&
                a.Date >= today &&
                a.Date < tomorrow)
            .Select(a => new
            {
                ClassId = a.ClassId!.Value,
                a.Status
            })
            .ToListAsync();

        var classAttendanceViewModels = classes
            .Select(c =>
            {
                var records = classAttendance
                    .Where(a => a.ClassId == c.Id)
                    .ToList();

                var present =
                    records.Count(a => a.Status == "Present");

                var absent =
                    records.Count(a => a.Status == "Absent");

                var late =
                    records.Count(a => a.Status == "Late");

                var totalRecorded =
                    present + absent + late;

                var percentage =
                    totalRecorded == 0
                        ? 0
                        : Math.Round(
                            (double)present
                            / totalRecorded
                            * 100,
                            1);

                var teacherName =
                    c.Teacher == null
                        ? "Not Assigned"
                        : c.Teacher.FullName;

                return new DashboardClassAttendanceViewModel
                {
                    ClassId = c.Id,
                    ClassName = c.ClassName,
                    Section = c.Section,
                    AcademicYear = c.AcademicYear,
                    TeacherName = teacherName,
                    StudentCount = c.Students.Count,
                    PresentCount = present,
                    AbsentCount = absent,
                    LateCount = late,
                    AttendancePercentage = percentage
                };
            })
            .ToList();


        // ========================================================
        // RECENT ATTENDANCE
        // ========================================================

        var recentAttendance =
            todayAttendance
                .OrderByDescending(a => a.Date)
                .Take(15)
                .Select(a => new DashboardAttendanceRecordViewModel
                {
                    Id = a.Id,
                    StudentName = a.StudentName,
                    ClassName = a.ClassName,
                    Section = a.Section,
                    Date = a.Date,
                    Status = a.Status,
                    Remarks = a.Remarks
                })
                .ToList();


        // ========================================================
        // GRADE STATISTICS
        // ========================================================

        var grades = await _context.Grades
            .AsNoTracking()
            .Where(g => g.MaxMarks > 0)
            .Select(g => new
            {
                g.MarksObtained,
                g.MaxMarks,
                g.SubjectName,
                g.AssessmentName,
                g.CreatedAt
            })
            .ToListAsync();

        var averageGradePercentage = grades.Count == 0
            ? 0
            : Math.Round(
                grades
                    .Select(g =>
                        (double)g.MarksObtained
                        / (double)g.MaxMarks
                        * 100)
                    .Average(),
                1);


        // ========================================================
        // PERFORMANCE BY SUBJECT
        // ========================================================

        var performanceData = grades
            .GroupBy(g => g.SubjectName)
            .Select(group => new PerformanceDataViewModel
            {
                Label = group.Key,

                Value = Math.Round(
                    group
                        .Select(g =>
                            (double)g.MarksObtained
                            / (double)g.MaxMarks
                            * 100)
                        .Average(),
                    1)
            })
            .OrderByDescending(x => x.Value)
            .ToList();


        // ========================================================
        // RECENT ACTIVITIES
        // ========================================================

        var recentGradeActivities = await _context.Grades
            .AsNoTracking()
            .Include(g => g.Student)
            .OrderByDescending(g => g.CreatedAt)
            .Take(5)
            .Select(g => new DashboardActivityViewModel
            {
                Title = "Grade recorded",

                Description =
                    g.Student.FirstName +
                    " " +
                    g.Student.LastName +
                    " received " +
                    g.MarksObtained.ToString() +
                    "/" +
                    g.MaxMarks.ToString() +
                    " in " +
                    g.SubjectName,

                Icon = "bi-award",

                CreatedAt = g.CreatedAt
            })
            .ToListAsync();

        var recentAttendanceActivities =
            await _context.Attendances
                .AsNoTracking()
                .Include(a => a.Student)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new DashboardActivityViewModel
                {
                    Title = "Attendance recorded",

                    Description =
                        a.Student.FirstName +
                        " " +
                        a.Student.LastName +
                        " marked " +
                        a.Status,

                    Icon =
                        a.Status == "Present"
                            ? "bi-check-circle"
                            : a.Status == "Absent"
                                ? "bi-x-circle"
                                : "bi-clock",

                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

        var recentActivities =
            recentGradeActivities
                .Concat(recentAttendanceActivities)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToList();


        // ========================================================
        // BUILD DASHBOARD
        // ========================================================

        return new DashboardViewModel
        {
            TotalStudents = totalStudents,

            TotalTeachers = totalTeachers,

            TotalClasses = totalClasses,

            AttendancePercentage = attendancePercentage,

            AverageGradePercentage =
                averageGradePercentage,

            TotalGrades = grades.Count,

            PresentToday = presentToday,

            AbsentToday = absentToday,

            LateToday = lateToday,

            AttendanceRecordsToday =
                attendanceRecordsToday,

            StudentsAtRisk = 0,

            AttendanceAlerts = 0,

            AIRecommendations = 0,

            PerformanceData =
                performanceData,

            ClassAttendance =
                classAttendanceViewModels,

            RecentAttendance =
                recentAttendance,

            RecentActivities =
                recentActivities
        };
    }
}