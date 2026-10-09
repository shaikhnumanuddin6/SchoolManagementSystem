using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolManagementSystem.Application.ViewModels.Classes;

public class ClassEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Class Name")]
    public string ClassName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Section { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Display(Name = "Academic Year")]
    public string AcademicYear { get; set; } = string.Empty;

    [Display(Name = "Assigned Teacher")]
    public int? TeacherId { get; set; }

    [Display(Name = "Room Number")]
    [Range(1, 99999)]
    public int? RoomNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SelectListItem> Teachers { get; set; } = new();
}