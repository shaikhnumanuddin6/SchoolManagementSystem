namespace SchoolManagementSystem.Application.ViewModels.Teachers;

public class TeacherDetailsViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Qualification { get; set; }

    public string? SpecializationDepartment { get; set; }

    public string? Phone { get; set; }

    public DateTime? HireDate { get; set; }

    public bool IsActive { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ClassCount { get; set; }

    public List<TeacherClassViewModel> Classes { get; set; } = new();
}


public class TeacherClassViewModel
{
    public int Id { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public int? RoomNumber { get; set; }

    public int StudentCount { get; set; }

    public bool IsActive { get; set; }
}