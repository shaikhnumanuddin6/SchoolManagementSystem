using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Domain.Entities;

public class Grade
{
    public int Id { get; set; }

    // ============================================================
    // SUBJECT INFORMATION
    // ============================================================

    [Required]
    [StringLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    // Foreign key to the Subjects table.
    // Nullable to preserve compatibility with existing grades.
    public int? SubjectId { get; set; }

    [ForeignKey(nameof(SubjectId))]
    public Subject? Subject { get; set; }

    // Backward-compatible string alias.
    // Renamed because Subject is now the navigation property.
    [NotMapped]
    public string SubjectDisplayName
    {
        get => SubjectName;
        set => SubjectName = value;
    }


    // ============================================================
    // ASSESSMENT INFORMATION
    // ============================================================

    [Required]
    [StringLength(100)]
    public string AssessmentName { get; set; } = string.Empty;

    [Required]
    [Range(0, 1000)]
    public decimal MarksObtained { get; set; }

    [Required]
    [Range(1, 1000)]
    public decimal MaxMarks { get; set; }

    // Backward-compatible alias for MaximumMarks.
    [NotMapped]
    public decimal MaximumMarks
    {
        get => MaxMarks;
        set => MaxMarks = value;
    }

    [StringLength(20)]
    public string? GradeLetter { get; set; }

    [StringLength(1000)]
    public string? Feedback { get; set; }

    // Backward-compatible alias for TeacherFeedback.
    [NotMapped]
    public string? TeacherFeedback
    {
        get => Feedback;
        set => Feedback = value;
    }

    [DataType(DataType.Date)]
    public DateTime ExamDate { get; set; } = DateTime.UtcNow;

    // Backward-compatible alias for AssessmentDate.
    [NotMapped]
    public DateTime AssessmentDate
    {
        get => ExamDate;
        set => ExamDate = value;
    }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // ============================================================
    // FOREIGN KEYS
    // ============================================================

    [Required]
    public int StudentId { get; set; }

    public int? ClassId { get; set; }


    // ============================================================
    // NAVIGATION PROPERTIES
    // ============================================================

    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    [ForeignKey(nameof(ClassId))]
    public SchoolClass? Class { get; set; }
}