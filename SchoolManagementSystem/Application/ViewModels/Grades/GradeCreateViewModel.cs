using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolManagementSystem.Application.ViewModels.Grades;

public class GradeCreateViewModel
{
    [Required(ErrorMessage = "Please select a student.")]
    public int? StudentId { get; set; }

    [Required(ErrorMessage = "Please select a subject.")]
    public int? SubjectId { get; set; }

    [Required(ErrorMessage = "Please select a class.")]
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