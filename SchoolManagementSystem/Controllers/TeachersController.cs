using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Teachers;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class TeachersController : Controller
{
    private readonly ITeacherService _teacherService;

    public TeachersController(
        ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    // ============================================================
    // TEACHER LIST
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? status)
    {
        var model =
            await _teacherService.GetTeachersAsync(
                searchTerm,
                status);

        return View(
            "~/Views/Teachers/Index.cshtml",
            model);
    }   
    // ============================================================
    // CREATE TEACHER
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model =
            await _teacherService.GetCreateViewModelAsync();

        return View(
            "~/Views/Teachers/Create.cshtml",
            model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        TeacherCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(
                "~/Views/Teachers/Create.cshtml",
                model);
        }

        var result =
            await _teacherService.CreateTeacherAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ??
                "Unable to create teacher.");

            return View(
                "~/Views/Teachers/Create.cshtml",
                model);
        }

        TempData["SuccessMessage"] =
            "Teacher account was created successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = result.TeacherId });
    }


    // ============================================================
    // TEACHER DETAILS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model =
            await _teacherService.GetDetailsAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(
            "~/Views/Teachers/Details.cshtml",
            model);
    }


    // ============================================================
    // EDIT TEACHER
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model =
            await _teacherService.GetEditViewModelAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(
            "~/Views/Teachers/Edit.cshtml",
            model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        TeacherEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(
                "~/Views/Teachers/Edit.cshtml",
                model);
        }

        var result =
            await _teacherService.UpdateTeacherAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ??
                "Unable to update teacher.");

            return View(
                "~/Views/Teachers/Edit.cshtml",
                model);
        }

        TempData["SuccessMessage"] =
            "Teacher information was updated successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.Id });
    }


    // ============================================================
    // ARCHIVE TEACHER
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        var result =
            await _teacherService.ArchiveTeacherAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ??
                "Unable to archive teacher.";

            return RedirectToAction(
                nameof(Index));
        }

        TempData["SuccessMessage"] =
            "Teacher was archived successfully.";

        return RedirectToAction(
            nameof(Index));
    }
}