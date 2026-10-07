
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider)
    {
        var context =
            serviceProvider.GetRequiredService<ApplicationDbContext>();

        var userManager =
            serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();


        // ========================================================
        // 1. ROLES
        // ========================================================

        string[] roles =
        {
            "Admin",
            "Teacher",
            "Student",
            "Parent"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult =
                    await roleManager.CreateAsync(
                        new IdentityRole(role));

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Unable to create role '{role}': {errors}");
                }
            }
        }


        // ========================================================
        // 2. ADMIN ACCOUNT
        //
        // Email:
        // admin@school.com
        //
        // Password:
        // Admin123!
        //
        // Role:
        // Admin
        // ========================================================

        var adminUser = await EnsureUserAsync(
            userManager,
            email: "admin@school.com",
            password: "Admin123!",
            firstName: "System",
            lastName: "Admin",
            role: "Admin");


        // ========================================================
        // 3. TEACHER ACCOUNT
        //
        // Email:
        // teacher@school.com
        //
        // Password:
        // Teacher123!
        //
        // Role:
        // Teacher
        // ========================================================

        var teacherUser = await EnsureUserAsync(
            userManager,
            email: "teacher@school.com",
            password: "Teacher123!",
            firstName: "Sarah",
            lastName: "Connor",
            role: "Teacher");


        // ========================================================
        // 4. TEACHER ENTITY
        // ========================================================

        var teacher =
            await context.Teachers
                .FirstOrDefaultAsync(
                    t => t.ApplicationUserId == teacherUser.Id);

        if (teacher == null)
        {
            teacher = new Teacher
            {
                FirstName = "Sarah",
                LastName = "Connor",
                EmployeeNumber = "TCH-1001",
                Qualification = "M.Ed, B.Sc Mathematics",
                SpecializationDepartment = "Mathematics",
                Phone = "+1 555-0101",
                HireDate = DateTime.UtcNow.AddYears(-2),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ApplicationUserId = teacherUser.Id
            };

            context.Teachers.Add(teacher);

            await context.SaveChangesAsync();
        }


        // ========================================================
        // 5. DEFAULT SCHOOL CLASS
        // ========================================================

        var defaultClass =
            await context.Classes.FirstOrDefaultAsync(c =>
                c.ClassName == "Grade 10" &&
                c.Section == "Section A" &&
                c.AcademicYear == "2025-2026");

        if (defaultClass == null)
        {
            defaultClass = new SchoolClass
            {
                ClassName = "Grade 10",
                Section = "Section A",
                AcademicYear = "2025-2026",
                RoomNumber = 101,
                TeacherId = teacher.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Classes.Add(defaultClass);

            await context.SaveChangesAsync();
        }
        else if (defaultClass.TeacherId != teacher.Id)
        {
            defaultClass.TeacherId = teacher.Id;

            await context.SaveChangesAsync();
        }


        // ========================================================
        // 6. STUDENT ACCOUNT
        //
        // Email:
        // student@school.com
        //
        // Password:
        // Student123!
        //
        // Role:
        // Student
        // ========================================================

        var studentUser = await EnsureUserAsync(
            userManager,
            email: "student@school.com",
            password: "Student123!",
            firstName: "Alex",
            lastName: "Johnson",
            role: "Student");


        // ========================================================
        // 7. STUDENT ENTITY
        // ========================================================

        var student =
            await context.Students
                .FirstOrDefaultAsync(
                    s => s.ApplicationUserId == studentUser.Id);

        if (student == null)
        {
            student = new Student
            {
                FirstName = "Alex",
                LastName = "Johnson",
                AdmissionNumber = "STD-2024-001",
                DateOfBirth = new DateTime(2009, 8, 14),
                Gender = "Male",
                EmergencyContact = "+1 555-0199",
                Address = "742 Evergreen Terrace",
                EnrollmentDate = DateTime.UtcNow.AddMonths(-6),
                Status = "Active",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ClassId = defaultClass.Id,
                ApplicationUserId = studentUser.Id
            };

            context.Students.Add(student);

            await context.SaveChangesAsync();
        }
        else if (student.ClassId != defaultClass.Id)
        {
            student.ClassId = defaultClass.Id;

            await context.SaveChangesAsync();
        }


        // ========================================================
        // 8. SAMPLE ATTENDANCE
        // ========================================================
        //
        // NOTE:
        // These properties are intentionally kept based on your
        // current DbInitializer. If your Attendance entity has
        // different property names, we will align this section
        // after reviewing Attendance.cs.
        //
        // ========================================================

        if (!await context.Attendances.AnyAsync(
                a => a.StudentId == student.Id))
        {
            context.Attendances.AddRange(
                new Attendance
                {
                    StudentId = student.Id,
                    ClassId = defaultClass.Id,
                    Date = DateTime.Today,
                    Status = AttendanceStatus.Present,
                    Remarks = "On time"
                },
                new Attendance
                {
                    StudentId = student.Id,
                    ClassId = defaultClass.Id,
                    Date = DateTime.Today.AddDays(-1),
                    Status = AttendanceStatus.Present,
                    Remarks = "On time"
                }
            );
        }


        // ========================================================
        // 9. SAMPLE GRADES
        // ========================================================
        //
        // NOTE:
        // These properties are intentionally kept based on your
        // current DbInitializer. We will verify them against
        // Grade.cs before making further changes.
        //
        // ========================================================

        if (!await context.Grades.AnyAsync(
                g => g.StudentId == student.Id))
        {
            context.Grades.AddRange(
                new Grade
                {
                    StudentId = student.Id,
                    ClassId = defaultClass.Id,
                    SubjectName = "Mathematics",
                    AssessmentName = "Mid-Term Examination",
                    MarksObtained = 88.50m,
                    MaxMarks = 100.00m,
                    GradeLetter = "A",
                    Feedback = "Excellent problem solving ability.",
                    ExamDate = DateTime.UtcNow.AddDays(-14)
                },
                new Grade
                {
                    StudentId = student.Id,
                    ClassId = defaultClass.Id,
                    SubjectName = "Physics",
                    AssessmentName = "Mid-Term Examination",
                    MarksObtained = 92.00m,
                    MaxMarks = 100.00m,
                    GradeLetter = "A+",
                    Feedback = "Great experimental understanding.",
                    ExamDate = DateTime.UtcNow.AddDays(-10)
                },
                new Grade
                {
                    StudentId = student.Id,
                    ClassId = defaultClass.Id,
                    SubjectName = "English Literature",
                    AssessmentName = "Term Essay",
                    MarksObtained = 85.00m,
                    MaxMarks = 100.00m,
                    GradeLetter = "A",
                    Feedback = "Well-structured arguments.",
                    ExamDate = DateTime.UtcNow.AddDays(-5)
                }
            );
        }


        // ========================================================
        // 10. SAVE ALL REMAINING CHANGES
        // ========================================================

        await context.SaveChangesAsync();
    }


    // ============================================================
    // ENSURE USER EXISTS
    // ============================================================

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        email = email.Trim().ToLowerInvariant();

        var user =
            await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,

                FirstName = firstName,
                LastName = lastName,

                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult =
                await userManager.CreateAsync(
                    user,
                    password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    createResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Unable to create user '{email}': {errors}");
            }
        }
        else
        {
            // Keep the seeded account active.
            if (!user.IsActive)
            {
                user.IsActive = true;

                var updateResult =
                    await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        updateResult.Errors.Select(
                            e => e.Description));

                    throw new InvalidOperationException(
                        $"Unable to update user '{email}': {errors}");
                }
            }
        }


        // ========================================================
        // ENSURE ROLE ASSIGNMENT
        // ========================================================

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult =
                await userManager.AddToRoleAsync(
                    user,
                    role);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    roleResult.Errors.Select(
                        e => e.Description));

                throw new InvalidOperationException(
                    $"Unable to assign role '{role}' to '{email}': {errors}");
            }
        }

        return user;
    }
}

