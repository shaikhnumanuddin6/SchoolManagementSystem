using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Application.ViewModels.Teachers;

public class TeacherEditViewModel
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


    public bool IsActive { get; set; }


    public string Status { get; set; } = "Active";
}