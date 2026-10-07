
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Application.ViewModels.Students;

public class StudentEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Admission Number")]
    public string AdmissionNumber { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [Phone]
    [StringLength(30)]
    [Display(Name = "Emergency Contact")]
    public string? EmergencyContact { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Class")]
    public int? ClassId { get; set; }

    public string Status { get; set; } = "Active";

    public bool IsActive { get; set; }

    public List<ClassFilterViewModel> Classes { get; set; } = new();
}

