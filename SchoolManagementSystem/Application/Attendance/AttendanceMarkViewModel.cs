
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolManagementSystem.Application.ViewModels.Attendance;

public class AttendanceMarkViewModel
{
    [Required(ErrorMessage = "Please select a class.")]
    [Display(Name = "Class")]
    public int? ClassId { get; set; }

    [Required(ErrorMessage = "Please select an attendance date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Attendance Date")]
    public DateTime Date { get; set; } = DateTime.Today;

    public List<AttendanceStudentEntryViewModel> Students { get; set; } = new();

    public List<SelectListItem> Classes { get; set; } = new();
}

public class AttendanceStudentEntryViewModel
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string AdmissionNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an attendance status.")]
    [RegularExpression(
        "^(Present|Absent|Late|Excused)$",
        ErrorMessage = "Please select a valid attendance status.")]
    public string Status { get; set; } = "Present";

    [StringLength(500)]
    public string? Remarks { get; set; }
}
