using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // ========================================================
        // 1. ROLES (Admin, Teacher, Student)
        // ========================================================
        string[] roles = { "Admin", "Teacher", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ========================================================
        // 2. ADMIN ACCOUNT
        // Email: admin@school.com
        // Password: Admin123!
        // Role: Admin
        // ========================================================
        var adminEmail = "admin@school.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                UserRole = "Admin",
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // ========================================================
        // 3. TEACHER ACCOUNT
        // Email: teacher@school.com
        // Password: Teacher123!
        // Role: Teacher
        // ========================================================
        var teacherEmail = "teacher@school.com";
        var teacherUser = await userManager.FindByEmailAsync(teacherEmail);
        if (teacherUser == null)
        {
            teacherUser = new ApplicationUser
            {
                UserName = teacherEmail,
                Email = teacherEmail,
                FullName = "Sarah Connor",
                UserRole = "Teacher",
                FirstName = "Sarah",
                LastName = "Connor",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(teacherUser, "Teacher123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(teacherUser, "Teacher");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(teacherUser, "Teacher"))
            {
                await userManager.AddToRoleAsync(teacherUser, "Teacher");
            }
        }

        // Ensure Teacher Entity exists
        var teacher = await context.Teachers.FirstOrDefaultAsync(t => t.ApplicationUserId == teacherUser.Id);
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
                ApplicationUserId = teacherUser.Id
            };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();
        }

        // ========================================================
        // 4. DEFAULT SCHOOL CLASS
        // ========================================================
        var defaultClass = await context.Classes.FirstOrDefaultAsync(c => c.ClassName == "Grade 10" && c.Section == "Section A");
        if (defaultClass == null)
        {
            defaultClass = new SchoolClass
            {
                ClassName = "Grade 10",
                Section = "Section A",
                AcademicYear = "2025-2026",
                RoomNumber = 101,
                TeacherId = teacher.Id,
                IsActive = true
            };
            context.Classes.Add(defaultClass);
            await context.SaveChangesAsync();
        }

        // ========================================================
        // 5. STUDENT ACCOUNT
        // Email: student@school.com
        // Password: Student123!
        // Role: Student
        // ========================================================
        var studentEmail = "student@school.com";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);
        if (studentUser == null)
        {
            studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FullName = "Alex Johnson",
                UserRole = "Student",
                FirstName = "Alex",
                LastName = "Johnson",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(studentUser, "Student123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(studentUser, "Student"))
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
        }

        // Ensure Student Entity exists
        var student = await context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == studentUser.Id);
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
                ClassId = defaultClass.Id,
                ApplicationUserId = studentUser.Id
            };
            context.Students.Add(student);
            await context.SaveChangesAsync();
        }

        // ========================================================
        // 6. SAMPLE ATTENDANCE & GRADES
        // ========================================================
        if (!await context.Attendances.AnyAsync(a => a.StudentId == student.Id))
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

        if (!await context.Grades.AnyAsync(g => g.StudentId == student.Id))
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

        await context.SaveChangesAsync();
    }
}
