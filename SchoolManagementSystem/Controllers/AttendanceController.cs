
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Attendance;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AttendanceController : Controller
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(
        IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    // ============================================================
    // GET: /Attendance
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] AttendanceListViewModel filter)
    {
        var model = await _attendanceService.GetListAsync(filter);

        return View(model);
    }

    // ============================================================
    // GET: /Attendance/Mark
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Mark(
        int? classId,
        DateTime? date)
    {
        var model = await _attendanceService
            .GetMarkViewModelAsync(classId, date);

        return View(model);
    }

    // ============================================================
    // POST: /Attendance/Mark
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark(
        AttendanceMarkViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await RefreshMarkFormAsync(model);
            return View(model);
        }

        var result = await _attendanceService.SaveAttendanceAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ??
                "Unable to save attendance.");

            await RefreshMarkFormAsync(model);

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Attendance saved successfully.";

        return RedirectToAction(
            nameof(Index),
            new
            {
                date = model.Date.ToString("yyyy-MM-dd"),
                classId = model.ClassId
            });
    }

    // ============================================================
    // REFRESH FORM DATA AFTER VALIDATION ERRORS
    // ============================================================

    private async Task RefreshMarkFormAsync(
        AttendanceMarkViewModel model)
    {
        var submittedStudents = model.Students ?? new();

        var freshModel = await _attendanceService
            .GetMarkViewModelAsync(model.ClassId, model.Date);

        // Preserve submitted choices only for students that the
        // service confirms belong to the selected active class.
        foreach (var student in freshModel.Students)
        {
            var submitted = submittedStudents.FirstOrDefault(
                s => s.StudentId == student.StudentId);

            if (submitted != null)
            {
                student.Status = submitted.Status;
                student.Remarks = submitted.Remarks;
            }
        }

        model.Classes = freshModel.Classes;
        model.Students = freshModel.Students;
    }
}
