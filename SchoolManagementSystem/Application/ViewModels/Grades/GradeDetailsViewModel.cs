namespace SchoolManagementSystem.Application.ViewModels.Grades;

public class GradeDetailsViewModel
{
    public int Id { get; set; }

    public string StudentName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;
    public string AssessmentName { get; set; } = string.Empty;

    public decimal MarksObtained { get; set; }
    public decimal MaxMarks { get; set; }

    public double Percentage { get; set; }

    public string? GradeLetter { get; set; }
    public string? Feedback { get; set; }

    public string? ClassName { get; set; }
    public DateTime ExamDate { get; set; }
    public DateTime CreatedAt { get; set; }
}