
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Domain.Entities;

public class Announcement
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string TargetAudience { get; set; } = "Everyone";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsPublished { get; set; } = false;

    [StringLength(450)]
    public string? CreatedByUserId { get; set; }
}
