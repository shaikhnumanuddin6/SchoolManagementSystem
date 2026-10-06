
namespace SchoolManagementSystem.Application.ViewModels;

public class DashboardViewModel
{
    // ============================================================
    // SUMMARY STATISTICS
    // ============================================================

    public int TotalStudents { get; set; }

    public int TotalTeachers { get; set; }

    public int TotalClasses { get; set; }

    public double AttendancePercentage { get; set; }


    // ============================================================
    // GRADE STATISTICS
    // ============================================================

    public double AverageGradePercentage { get; set; }

    public int TotalGrades { get; set; }


    // ============================================================
    // ATTENDANCE
    // ============================================================

    public int PresentToday { get; set; }

    public int AbsentToday { get; set; }

    public int LateToday { get; set; }


    // ============================================================
    // AI INSIGHTS
    // ============================================================

    public int StudentsAtRisk { get; set; }

    public int AttendanceAlerts { get; set; }

    public int AIRecommendations { get; set; }


    // ============================================================
    // RECENT ACTIVITIES
    // ============================================================

    public List<DashboardActivityViewModel> RecentActivities { get; set; }
        = new();


    // ============================================================
    // PERFORMANCE DATA
    // ============================================================

    public List<PerformanceDataViewModel> PerformanceData { get; set; }
        = new();
}


// ================================================================
// RECENT ACTIVITY
// ================================================================

public class DashboardActivityViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Icon { get; set; } = "bi-info-circle";

    public DateTime CreatedAt { get; set; }
}


// ================================================================
// PERFORMANCE DATA
// ================================================================

public class PerformanceDataViewModel
{
    public string Label { get; set; } = string.Empty;

    public double Value { get; set; }
}

