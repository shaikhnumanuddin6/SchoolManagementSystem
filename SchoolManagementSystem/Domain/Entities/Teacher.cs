using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Domain.Entities;

public class Teacher
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    [Required]
    [StringLength(50)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Qualification { get; set; }

    [StringLength(150)]
    public string? SpecializationDepartment { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key
    [Required]
    [StringLength(450)]
    public string ApplicationUserId { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey(nameof(ApplicationUserId))]
    public ApplicationUser User { get; set; } = null!;

    public ICollection<SchoolClass> Classes { get; set; } = new List<SchoolClass>();
}