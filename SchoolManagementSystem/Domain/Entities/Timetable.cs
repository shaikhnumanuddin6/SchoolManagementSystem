using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Domain.Entities;

public class Timetable
{
    public int Id { get; set; }

    [Required]
    public int ClassId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    [Range(1, 7)]
    [Display(Name = "Day of Week")]
    public int DayOfWeek { get; set; }

    [Required]
    [Display(Name = "Start Time")]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Display(Name = "End Time")]
    public TimeSpan EndTime { get; set; }

    [StringLength(50)]
    public string? RoomNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(ClassId))]
    public SchoolClass Class { get; set; } = null!;

    [ForeignKey(nameof(SubjectId))]
    public Subject Subject { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public Teacher Teacher { get; set; } = null!;
}

 