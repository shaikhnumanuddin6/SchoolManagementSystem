using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Domain.Entities;

public class Grade
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    // Helper alias for Subject
    [NotMapped]
    public string Subject
    {
        get => SubjectName;
        set => SubjectName = value;
    }

    [Required]
    [StringLength(100)]
    public string AssessmentName { get; set; } = string.Empty;

    [Required]
    [Range(0, 1000)]
    public decimal MarksObtained { get; set; }

    [Required]
    [Range(1, 1000)]
    public decimal MaxMarks { get; set; }

    // Helper alias for MaximumMarks
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

    // Helper alias for TeacherFeedback
    [NotMapped]
    public string? TeacherFeedback
    {
        get => Feedback;
        set => Feedback = value;
    }

    [DataType(DataType.Date)]
    public DateTime ExamDate { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public DateTime AssessmentDate
    {
        get => ExamDate;
        set => ExamDate = value;
    }

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