namespace SchoolManagementSystem.Application.ViewModels.Classes;

public class ClassListViewModel
{
    public List<ClassListItemViewModel> Classes { get; set; } = new();

    public string? SearchTerm { get; set; }

    public string? AcademicYear { get; set; }

    public string? Status { get; set; }

    public List<string> AcademicYears { get; set; } = new();

    public int TotalClasses { get; set; }

    public int ActiveClasses { get; set; }

    public int InactiveClasses { get; set; }
}

public class ClassListItemViewModel
{
    public int Id { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string Section { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public int? RoomNumber { get; set; }

    public int? TeacherId { get; set; }

    public string TeacherName { get; set; } = "Not Assigned";

    public int StudentCount { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}