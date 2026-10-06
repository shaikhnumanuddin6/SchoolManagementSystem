using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Domain.Entities;

public class SchoolClass
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string ClassName { get; set; } = string.Empty;

    // Helper property to maintain backwards compatibility if needed
    [NotMapped]
    public string Name
    {
        get => ClassName;
        set => ClassName = value;
    }

    [Required]
    [StringLength(20)]
    public string Section { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string AcademicYear { get; set; } = string.Empty;

    public int? TeacherId { get; set; }

    public int? RoomNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TeacherId))]
    public Teacher? Teacher { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}