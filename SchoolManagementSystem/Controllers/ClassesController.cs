using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Classes;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class ClassesController : Controller
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }

    // ============================================================
    // CLASS LIST
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? academicYear,
        string? status)
    {
        var model = await _classService.GetClassesAsync(
            searchTerm,
            academicYear,
            status);

        return View("~/Views/Classes/Index.cshtml", model);
    }

    // ============================================================
    // CREATE - GET
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model =
            await _classService.GetCreateViewModelAsync();

        return View("~/Views/Classes/Create.cshtml", model);
    }

    // ============================================================
    // CREATE - POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Teachers =
                await _classService.GetTeacherOptionsAsync(
                    model.TeacherId);

            return View("~/Views/Classes/Create.cshtml", model);
        }

        var result =
            await _classService.CreateClassAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Unable to create class.");

            model.Teachers =
                await _classService.GetTeacherOptionsAsync(
                    model.TeacherId);

            return View("~/Views/Classes/Create.cshtml", model);
        }

        TempData["SuccessMessage"] =
            "Class created successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = result.ClassId });
    }

    // ============================================================
    // DETAILS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _classService.GetDetailsAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View("~/Views/Classes/Details.cshtml", model);
    }

    // ============================================================
    // EDIT - GET
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _classService.GetEditViewModelAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View("~/Views/Classes/Edit.cshtml", model);
    }

    // ============================================================
    // EDIT - POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ClassEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.Teachers =
                await _classService.GetTeacherOptionsAsync(
                    model.TeacherId);

            return View("~/Views/Classes/Edit.cshtml", model);
        }

        var result =
            await _classService.UpdateClassAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Unable to update class.");

            model.Teachers =
                await _classService.GetTeacherOptionsAsync(
                    model.TeacherId);

            return View("~/Views/Classes/Edit.cshtml", model);
        }

        TempData["SuccessMessage"] =
            "Class updated successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.Id });
    }

    // ============================================================
    // ARCHIVE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        var result =
            await _classService.ArchiveClassAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ?? "Unable to archive class.";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Class archived successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // RESTORE
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var result =
            await _classService.RestoreClassAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ?? "Unable to restore class.";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Class restored successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}