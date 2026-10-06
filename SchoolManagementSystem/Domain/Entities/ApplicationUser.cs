using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserRole { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Student? Student { get; set; }

    public Teacher? Teacher { get; set; }
}