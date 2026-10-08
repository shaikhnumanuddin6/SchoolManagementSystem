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
    // TODAY'S ATTENDANCE
    // ============================================================

    public int PresentToday { get; set; }

    public int AbsentToday { get; set; }

    public int LateToday { get; set; }

    public int AttendanceRecordsToday { get; set; }


    // ============================================================
    // AI INSIGHTS
    // ============================================================

    public int StudentsAtRisk { get; set; }

    public int AttendanceAlerts { get; set; }

    public int AIRecommendations { get; set; }


    // ============================================================
    // PERFORMANCE DATA
    // ============================================================

    public List<PerformanceDataViewModel> PerformanceData { get; set; }
        = new();


    // ============================================================
    // CLASS-WISE ATTENDANCE
    // ============================================================

    public List<DashboardClassAttendanceViewModel> ClassAttendance { get; set; }
        = new();


    // ============================================================
    // RECENT ATTENDANCE
    // ============================================================

    public List<DashboardAttendanceRecordViewModel> RecentAttendance { get; set; }
        = new();


    // ============================================================
    // RECENT ACTIVITIES
    // ============================================================

    public List<DashboardActivityViewModel> RecentActivities { get; set; }
        = new();
}


// ================================================================
// CLASS ATTENDANCE
// ================================================================

public class DashboardClassAttendanceViewModel
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public string TeacherName { get; set; } = "Not Assigned";

    public int StudentCount { get; set; }

    public int PresentCount { get; set; }

    public int AbsentCount { get; set; }

    public int LateCount { get; set; }

    public double AttendancePercentage { get; set; }
}


// ================================================================
// RECENT ATTENDANCE
// ================================================================

public class DashboardAttendanceRecordViewModel
{
    public int Id { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Remarks { get; set; }
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