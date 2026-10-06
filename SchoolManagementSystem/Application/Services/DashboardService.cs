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

        // ========================================================
        // BASIC COUNTS
        // ========================================================

        var totalStudents = await _context.Students.CountAsync();
        var totalTeachers = await _context.Teachers.CountAsync();
        var totalClasses = await _context.Classes.CountAsync();

        // ========================================================
        // TODAY'S ATTENDANCE
        // ========================================================

        var todayAttendance = await _context.Attendances
            .Where(a => a.Date >= today && a.Date < today.AddDays(1))
            .ToListAsync();

        var presentToday = todayAttendance.Count(a => a.Status == "Present");
        var absentToday = todayAttendance.Count(a => a.Status == "Absent");
        var lateToday = todayAttendance.Count(a => a.Status == "Late");

        // ========================================================
        // ATTENDANCE PERCENTAGE
        // ========================================================

        var attendancePercentage = todayAttendance.Count == 0
            ? 0
            : Math.Round((double)presentToday / todayAttendance.Count * 100, 1);

        // ========================================================
        // GRADES
        // ========================================================

        var grades = await _context.Grades
            .Select(g => new
            {
                g.MarksObtained,
                MaximumMarks = g.MaxMarks
            })
            .ToListAsync();

        double averageGradePercentage = 0;

        if (grades.Count > 0)
        {
            averageGradePercentage = Math.Round(
                grades
                    .Where(g => g.MaximumMarks > 0)
                    .Select(g => (double)g.MarksObtained / (double)g.MaximumMarks * 100)
                    .DefaultIfEmpty(0)
                    .Average(),
                1);
        }

        // ========================================================
        // PERFORMANCE BY SUBJECT
        // ========================================================

        var rawGrades = await _context.Grades
            .Where(g => g.MaxMarks > 0)
            .Select(g => new
            {
                Subject = g.SubjectName,
                g.MarksObtained,
                MaximumMarks = g.MaxMarks
            })
            .ToListAsync();

        var performanceData = rawGrades
            .GroupBy(g => g.Subject)
            .Select(group => new PerformanceDataViewModel
            {
                Label = group.Key,
                Value = Math.Round(group.Average(g => (double)g.MarksObtained / (double)g.MaximumMarks * 100), 1)
            })
            .OrderByDescending(x => x.Value)
            .Take(6)
            .ToList();

        // ========================================================
        // BUILD DASHBOARD MODEL
        // ========================================================

        return new DashboardViewModel
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalClasses = totalClasses,
            AttendancePercentage = attendancePercentage,
            AverageGradePercentage = averageGradePercentage,
            TotalGrades = grades.Count,
            PresentToday = presentToday,
            AbsentToday = absentToday,
            LateToday = lateToday,
            StudentsAtRisk = 0,
            AttendanceAlerts = 0,
            AIRecommendations = 0,
            PerformanceData = performanceData,
            RecentActivities = new List<DashboardActivityViewModel>
            {
                new()
                {
                    Title = "Dashboard loaded",
                    Description = "School statistics were updated successfully.",
                    Icon = "bi-check-circle",
                    CreatedAt = DateTime.Now
                }
            }
        };
    }
}