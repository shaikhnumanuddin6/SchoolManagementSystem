
namespace SchoolManagementSystem.Application.ViewModels.Students;

public class StudentListViewModel
{
    public List<StudentListItemViewModel> Students { get; set; } = new();

    public string? SearchTerm { get; set; }

    public int? ClassId { get; set; }

    public string? Status { get; set; }

    public List<ClassFilterViewModel> Classes { get; set; } = new();

    public int TotalStudents { get; set; }

    public int ActiveStudents { get; set; }

    public int InactiveStudents { get; set; }
}

public class StudentListItemViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string AdmissionNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? ClassName { get; set; }

    public string? Section { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime EnrollmentDate { get; set; }
}

public class ClassFilterViewModel
{
    public int Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}

