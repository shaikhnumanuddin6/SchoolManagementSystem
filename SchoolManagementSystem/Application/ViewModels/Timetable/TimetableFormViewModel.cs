
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolManagementSystem.Application.ViewModels.Timetable;

public class TimetableFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a class.")]
    [Display(Name = "Class")]
    public int ClassId { get; set; }

    [Required(ErrorMessage = "Please select a subject.")]
    [Display(Name = "Subject")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "Please select a teacher.")]
    [Display(Name = "Teacher")]
    public int TeacherId { get; set; }

    [Required(ErrorMessage = "Please select a day.")]
    [Range(1, 7, ErrorMessage = "Please select a valid day.")]
    [Display(Name = "Day of Week")]
    public int DayOfWeek { get; set; }

    [Required(ErrorMessage = "Please enter the start time.")]
    [Display(Name = "Start Time")]
    public TimeSpan StartTime { get; set; } = new(9, 0, 0);

    [Required(ErrorMessage = "Please enter the end time.")]
    [Display(Name = "End Time")]
    public TimeSpan EndTime { get; set; } = new(10, 0, 0);

    [StringLength(50)]
    [Display(Name = "Room Number")]
    public string? RoomNumber { get; set; }

    public List<SelectListItem> Classes { get; set; } = new();

    public List<SelectListItem> Subjects { get; set; } = new();

    public List<SelectListItem> Teachers { get; set; } = new();
}
