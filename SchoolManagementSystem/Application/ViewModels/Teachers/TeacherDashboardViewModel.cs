namespace SchoolManagementSystem.Application.ViewModels.Teachers;

public class TeacherDashboardViewModel
{
    public string TeacherName { get; set; } = string.Empty;

    public int AssignedClasses { get; set; }

    public int TotalStudents { get; set; }

    public double AttendancePercentage { get; set; }

    public int PresentToday { get; set; }

    public int AbsentToday { get; set; }

    public int LateToday { get; set; }

    public int AttendanceRecordsToday { get; set; }

    public int TotalGrades { get; set; }

    public double AverageGradePercentage { get; set; }

    public List<TeacherClassDashboardViewModel> Classes { get; set; }
        = new();

    public List<TeacherAttendanceViewModel> RecentAttendance { get; set; }
        = new();

    public List<TeacherGradeViewModel> RecentGrades { get; set; }
        = new();

    public List<TeacherPerformanceViewModel> PerformanceData { get; set; }
        = new();
}


// ============================================================
// CLASS DASHBOARD
// ============================================================

public class TeacherClassDashboardViewModel
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public int StudentCount { get; set; }

    public int PresentCount { get; set; }

    public int AbsentCount { get; set; }

    public int LateCount { get; set; }

    public double AttendancePercentage { get; set; }
}


// ============================================================
// ATTENDANCE
// ============================================================

public class TeacherAttendanceViewModel
{
    public int Id { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string? Remarks { get; set; }
}


// ============================================================
// GRADES
// ============================================================

public class TeacherGradeViewModel
{
    public int Id { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public string AssessmentName { get; set; } = string.Empty;

    public decimal MarksObtained { get; set; }

    public decimal MaxMarks { get; set; }

    public double Percentage { get; set; }

    public DateTime ExamDate { get; set; }
}


// ============================================================
// PERFORMANCE
// ============================================================

public class TeacherPerformanceViewModel
{
    public string SubjectName { get; set; } = string.Empty;

    public double Percentage { get; set; }
}