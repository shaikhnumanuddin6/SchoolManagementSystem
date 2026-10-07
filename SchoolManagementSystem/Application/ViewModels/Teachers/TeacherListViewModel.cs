namespace SchoolManagementSystem.Application.ViewModels.Teachers;

public class TeacherListViewModel
{
    public List<TeacherListItemViewModel> Teachers { get; set; } = new();

    public string? SearchTerm { get; set; }

    public string? Status { get; set; }

    public int TotalTeachers { get; set; }

    public int ActiveTeachers { get; set; }

    public int InactiveTeachers { get; set; }
}


public class TeacherListItemViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Qualification { get; set; }

    public string? SpecializationDepartment { get; set; }

    public string? Phone { get; set; }

    public DateTime HireDate { get; set; }

    public bool IsActive { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ClassCount { get; set; }
}