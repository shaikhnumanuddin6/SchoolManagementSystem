namespace SchoolManagementSystem.Application.ViewModels.Subjects;

public class SubjectListViewModel
{
    public List<SubjectListItemViewModel> Subjects { get; set; } = new();

    public string? SearchTerm { get; set; }

    public string? Status { get; set; }

    public int TotalSubjects { get; set; }

    public int ActiveSubjects { get; set; }

    public int InactiveSubjects { get; set; }
}

public class SubjectListItemViewModel
{
    public int Id { get; set; }

    public string SubjectCode { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}