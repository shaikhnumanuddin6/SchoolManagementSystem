namespace SchoolManagementSystem.Application.ViewModels;

public class StudentDashboardViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public double AttendancePercentage { get; set; }
    public int TotalAttendanceDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public double AverageGradePercentage { get; set; }
    public List<StudentGradeViewModel> Grades { get; set; } = new();
    public List<StudentAttendanceViewModel> RecentAttendances { get; set; } = new();
}

public class StudentGradeViewModel
{
    public string SubjectName { get; set; } = string.Empty;
    public string AssessmentName { get; set; } = string.Empty;
    public decimal MarksObtained { get; set; }
    public decimal MaxMarks { get; set; }
    public double Percentage => MaxMarks > 0 ? (double)(MarksObtained / MaxMarks * 100) : 0;
    public string? GradeLetter { get; set; }
    public string? Feedback { get; set; }
    public DateTime ExamDate { get; set; }
}

public class StudentAttendanceViewModel
{
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
