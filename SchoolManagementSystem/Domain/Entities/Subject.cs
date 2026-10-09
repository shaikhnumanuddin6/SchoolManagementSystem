using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Domain.Entities;

public class Subject
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Subject Code")]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Subject Name")]
    public string SubjectName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}