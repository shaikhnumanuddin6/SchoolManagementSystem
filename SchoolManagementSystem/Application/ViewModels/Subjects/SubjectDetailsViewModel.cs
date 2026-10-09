namespace SchoolManagementSystem.Application.ViewModels.Subjects;

public class SubjectDetailsViewModel
{
    public int Id { get; set; }

    public string SubjectCode { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}