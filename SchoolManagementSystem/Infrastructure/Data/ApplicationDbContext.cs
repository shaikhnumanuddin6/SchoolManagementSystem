using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ==============================
    // DbSets
    // ==============================

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Grade> Grades => Set<Grade>();

    // ==============================
    // EF Core Model Configuration
    // ==============================

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // MUST be called first when extending IdentityDbContext
        base.OnModelCreating(builder);

        ConfigureRelationships(builder);
        ConfigureIndexes(builder);
        ConfigureDecimalProperties(builder);
    }

    // ==============================
    // Relationships
    // ==============================

    private static void ConfigureRelationships(ModelBuilder builder)
    {
        // ApplicationUser -> Student (1:1)
        builder.Entity<Student>()
            .Property(s => s.ApplicationUserId)
            .HasMaxLength(450);

        builder.Entity<Student>()
            .HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApplicationUser -> Teacher (1:1)
        builder.Entity<Teacher>()
            .Property(t => t.ApplicationUserId)
            .HasMaxLength(450);

        builder.Entity<Teacher>()
            .HasOne(t => t.User)
            .WithOne(u => u.Teacher)
            .HasForeignKey<Teacher>(t => t.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Teacher -> SchoolClass (1:Many)
        builder.Entity<SchoolClass>()
            .HasOne(c => c.Teacher)
            .WithMany(t => t.Classes)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // SchoolClass -> Student (1:Many)
        builder.Entity<Student>()
            .HasOne(s => s.Class)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // Student -> Attendance (1:Many)
        builder.Entity<Attendance>()
            .HasOne(a => a.Student)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // SchoolClass -> Attendance (1:Many)
        builder.Entity<Attendance>()
            .HasOne(a => a.Class)
            .WithMany(c => c.Attendances)
            .HasForeignKey(a => a.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // Student -> Grade (1:Many)
        builder.Entity<Grade>()
            .HasOne(g => g.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // SchoolClass -> Grade (1:Many)
        builder.Entity<Grade>()
            .HasOne(g => g.Class)
            .WithMany(c => c.Grades)
            .HasForeignKey(g => g.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    // ==============================
    // Indexes
    // ==============================

    private static void ConfigureIndexes(ModelBuilder builder)
    {
        // Student Indexes
        builder.Entity<Student>()
            .HasIndex(s => s.AdmissionNumber)
            .IsUnique();

        builder.Entity<Student>()
            .HasIndex(s => s.ApplicationUserId)
            .IsUnique();

        // Teacher Indexes
        builder.Entity<Teacher>()
            .HasIndex(t => t.EmployeeNumber)
            .IsUnique();

        builder.Entity<Teacher>()
            .HasIndex(t => t.ApplicationUserId)
            .IsUnique();

        // School Class Composite Index
        builder.Entity<SchoolClass>()
            .HasIndex(c => new
            {
                c.ClassName,
                c.Section,
                c.AcademicYear
            })
            .IsUnique();

        // Attendance Composite Index
        builder.Entity<Attendance>()
            .HasIndex(a => new
            {
                a.StudentId,
                a.Date
            })
            .IsUnique();

        // Grade Index
        builder.Entity<Grade>()
            .HasIndex(g => new
            {
                g.StudentId,
                g.SubjectName,
                g.AssessmentName
            });
    }

    // ==============================
    // Decimal Precision Configuration
    // ==============================

    private static void ConfigureDecimalProperties(ModelBuilder builder)
    {
        builder.Entity<Grade>()
            .Property(g => g.MarksObtained)
            .HasPrecision(6, 2);

        builder.Entity<Grade>()
            .Property(g => g.MaxMarks)
            .HasPrecision(6, 2);
    }
}