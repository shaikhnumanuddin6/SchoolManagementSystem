
namespace SchoolManagementSystem.Application.ViewModels.Students;

public class StudentDetailsViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string AdmissionNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? EmergencyContact { get; set; }

    public string? Address { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string? ClassName { get; set; }

    public string? Section { get; set; }

    public string? AcademicYear { get; set; }

    public int AttendanceCount { get; set; }

    public int PresentCount { get; set; }

    public double AttendancePercentage { get; set; }

    public int GradeCount { get; set; }

    public double AverageGradePercentage { get; set; }
}

