USE [SchoolManagementDb];
GO

SELECT 'Students' AS TableName, COUNT(*) AS TotalRecords FROM dbo.Students
UNION ALL
SELECT 'Teachers', COUNT(*) FROM dbo.Teachers
UNION ALL
SELECT 'Classes', COUNT(*) FROM dbo.Classes
UNION ALL
SELECT 'Subjects', COUNT(*) FROM dbo.Subjects
UNION ALL
SELECT 'Attendance', COUNT(*) FROM dbo.Attendances
UNION ALL
SELECT 'Grades', COUNT(*) FROM dbo.Grades
UNION ALL
SELECT 'Identity Users', COUNT(*) FROM dbo.AspNetUsers
UNION ALL
SELECT 'Identity Roles', COUNT(*) FROM dbo.AspNetRoles;