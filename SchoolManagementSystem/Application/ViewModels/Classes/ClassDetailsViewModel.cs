namespace SchoolManagementSystem.Application.ViewModels.Classes;

public class ClassDetailsViewModel
{
    public int Id { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public int? RoomNumber { get; set; }

    public int? TeacherId { get; set; }

    public string TeacherName { get; set; } = "Not Assigned";

    public string? TeacherEmail { get; set; }

    public int StudentCount { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<ClassStudentViewModel> Students { get; set; } = new();
}

public class ClassStudentViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string AdmissionNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}