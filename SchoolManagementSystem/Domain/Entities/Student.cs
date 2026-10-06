using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Domain.Entities;

public class Student
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
    public string AdmissionNumber { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [StringLength(30)]
    public string? EmergencyContact { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Active";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    [Required]
    [StringLength(450)]
    public string ApplicationUserId { get; set; } = string.Empty;

    public int? ClassId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ApplicationUserId))]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(ClassId))]
    public SchoolClass? Class { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}