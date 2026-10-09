using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolManagementSystem.Application.ViewModels.Grades;

public class GradeListViewModel
{
    public List<GradeListItemViewModel> Grades { get; set; } = new();

    public string? SearchTerm { get; set; }
    public int? ClassId { get; set; }
    public int? StudentId { get; set; }
    public int? SubjectId { get; set; }

    public List<SelectListItem> Classes { get; set; } = new();
    public List<SelectListItem> Students { get; set; } = new();
    public List<SelectListItem> Subjects { get; set; } = new();

    public int TotalGrades { get; set; }
    public double AveragePercentage { get; set; }
}

public class GradeListItemViewModel
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
    public string? ClassName { get; set; }
    public DateTime ExamDate { get; set; }
}
