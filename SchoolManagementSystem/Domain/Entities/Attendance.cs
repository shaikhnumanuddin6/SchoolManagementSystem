using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Domain.Entities;

public class Attendance
{
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    // Helper property to maintain backwards compatibility with existing queries
    [NotMapped]
    public DateTime AttendanceDate
    {
        get => Date;
        set => Date = value;
    }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = AttendanceStatus.Present;

    [StringLength(500)]
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    [Required]
    public int StudentId { get; set; }

    public int? ClassId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    [ForeignKey(nameof(ClassId))]
    public SchoolClass? Class { get; set; }
}