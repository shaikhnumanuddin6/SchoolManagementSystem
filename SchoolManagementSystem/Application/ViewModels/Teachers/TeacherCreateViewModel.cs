using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Application.ViewModels.Teachers;

public class TeacherCreateViewModel
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
    [Display(Name = "Employee Number")]
    public string EmployeeNumber { get; set; } = string.Empty;


    [StringLength(150)]
    public string? Qualification { get; set; }


    [StringLength(150)]
    [Display(Name = "Specialization / Department")]
    public string? SpecializationDepartment { get; set; }


    [Phone]
    [StringLength(30)]
    public string? Phone { get; set; }


    [DataType(DataType.Date)]
    [Display(Name = "Hire Date")]
    public DateTime? HireDate { get; set; }


    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;


    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;


    [Required]
    [Compare(nameof(Password))]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;


    public bool IsActive { get; set; } = true;
}