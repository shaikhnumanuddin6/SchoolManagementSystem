
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Application.ViewModels.Students;

public class StudentCreateViewModel
{
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

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password))]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Class")]
    public int? ClassId { get; set; }

    public string Status { get; set; } = "Active";

    public List<ClassFilterViewModel> Classes { get; set; } = new();
}

