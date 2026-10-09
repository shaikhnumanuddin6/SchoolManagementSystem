using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolManagementSystem.Application.ViewModels.Grades;

public class GradeEditViewModel
{
    public int Id { get; set; }

    [Required]
    public int? StudentId { get; set; }

    [Required]
    public int? SubjectId { get; set; }

    [Required]
    public int? ClassId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Assessment Name")]
    public string AssessmentName { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0", "1000")]
    [Display(Name = "Marks Obtained")]
    public decimal MarksObtained { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "1000")]
    [Display(Name = "Maximum Marks")]
    public decimal MaxMarks { get; set; } = 100;

    [StringLength(1000)]
    public string? Feedback { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Exam Date")]
    public DateTime ExamDate { get; set; } = DateTime.Today;

    public List<SelectListItem> Students { get; set; } = new();
    public List<SelectListItem> Classes { get; set; } = new();
    public List<SelectListItem> Subjects { get; set; } = new();
}