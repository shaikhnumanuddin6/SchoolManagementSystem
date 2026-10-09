
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.ViewModels;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    // ============================================================
    // STUDENT PORTAL / DASHBOARD
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        // --------------------------------------------------------
        // Get currently logged-in Identity user
        // --------------------------------------------------------

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }


        // --------------------------------------------------------
        // Find Student entity linked to this Identity user
        // --------------------------------------------------------

        var student = await _context.Students
            .AsNoTracking()
            .Include(s => s.Class)
                .ThenInclude(c => c!.Teacher)
            .Include(s => s.Attendances)
            .Include(s => s.Grades)
            .FirstOrDefaultAsync(
                s => s.ApplicationUserId == user.Id);


        // --------------------------------------------------------
        // Student entity has not been assigned yet
        // --------------------------------------------------------

        if (student == null)
        {
            var userFullName =
                $"{user.FirstName} {user.LastName}".Trim();

            if (string.IsNullOrWhiteSpace(userFullName))
            {
                userFullName = user.Email ?? "Student";
            }

            return View(
                "~/Views/Students/Dashboard.cshtml",
                new StudentDashboardViewModel
                {
                    StudentName = userFullName,
                    AdmissionNumber = "Pending Assignment",
                    ClassName = "Not Assigned",
                    Section = "-",
                    TeacherName = "Not Assigned"
                });
        }


        // ========================================================
        // ATTENDANCE CALCULATIONS
        // ========================================================

        var totalAttendance =
            student.Attendances.Count;

        var presentDays =
            student.Attendances.Count(
                a => a.Status == AttendanceStatus.Present);

        var absentDays =
            student.Attendances.Count(
                a => a.Status == AttendanceStatus.Absent);

        var lateDays =
            student.Attendances.Count(
                a => a.Status == AttendanceStatus.Late);

        var attendancePercentage =
            totalAttendance > 0
                ? Math.Round(
                    (double)presentDays /
                    totalAttendance *
                    100,
                    1)
                : 0;


        // ========================================================
        // GRADE CALCULATIONS
        // ========================================================

        var validGrades = student.Grades
            .Where(g => g.MaxMarks > 0)
            .ToList();

        var averageGradePercentage =
            validGrades.Count > 0
                ? Math.Round(
                    validGrades.Average(g =>
                        (double)g.MarksObtained /
                        (double)g.MaxMarks *
                        100),
                    1)
                : 0;


        // ========================================================
        // STUDENT DASHBOARD VIEW MODEL
        // ========================================================

        var viewModel = new StudentDashboardViewModel
        {
            StudentName = student.FullName,

            AdmissionNumber =
                student.AdmissionNumber,

            ClassName =
                student.Class?.ClassName ??
                "Not Assigned",

            Section =
                student.Class?.Section ??
                "-",

            TeacherName =
                student.Class?.Teacher?.FullName ??
                "Not Assigned",

            AttendancePercentage =
                attendancePercentage,

            TotalAttendanceDays =
                totalAttendance,

            PresentDays =
                presentDays,

            AbsentDays =
                absentDays,

            LateDays =
                lateDays,

            AverageGradePercentage =
                averageGradePercentage,


            // ----------------------------------------------------
            // Grades
            // ----------------------------------------------------

            Grades = student.Grades
                .OrderByDescending(g => g.ExamDate)
                .Select(g => new StudentGradeViewModel
                {
                    SubjectName =
                        g.SubjectName,

                    AssessmentName =
                        g.AssessmentName,

                    MarksObtained =
                        g.MarksObtained,

                    MaxMarks =
                        g.MaxMarks,

                    GradeLetter =
                        g.GradeLetter,

                    Feedback =
                        g.Feedback,

                    ExamDate =
                        g.ExamDate
                })
                .ToList(),


            // ----------------------------------------------------
            // Recent Attendance
            // ----------------------------------------------------

            RecentAttendances =
                student.Attendances
                    .OrderByDescending(a => a.Date)
                    .Take(10)
                    .Select(a => new StudentAttendanceViewModel
                    {
                        Date = a.Date,

                        Status =
                            a.Status.ToString(),

                        Remarks =
                            a.Remarks
                    })
                    .ToList()
        };


        return View(
            "~/Views/Students/Dashboard.cshtml",
            viewModel);
    }
}

