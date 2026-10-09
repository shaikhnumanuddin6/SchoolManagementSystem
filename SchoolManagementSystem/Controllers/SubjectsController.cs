using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Subjects;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class SubjectsController : Controller
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    // ============================================================
    // SUBJECT LIST
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        string? status)
    {
        var model = await _subjectService.GetSubjectsAsync(
            searchTerm,
            status);

        return View("~/Views/Subjects/Index.cshtml", model);
    }

    // ============================================================
    // CREATE - GET
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model =
            await _subjectService.GetCreateViewModelAsync();

        return View("~/Views/Subjects/Create.cshtml", model);
    }

    // ============================================================
    // CREATE - POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SubjectCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Subjects/Create.cshtml", model);
        }

        var result =
            await _subjectService.CreateSubjectAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Unable to create subject.");

            return View("~/Views/Subjects/Create.cshtml", model);
        }

        TempData["SuccessMessage"] =
            "Subject created successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = result.SubjectId });
    }

    // ============================================================
    // DETAILS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model =
            await _subjectService.GetDetailsAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View("~/Views/Subjects/Details.cshtml", model);
    }

    // ============================================================
    // EDIT - GET
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model =
            await _subjectService.GetEditViewModelAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View("~/Views/Subjects/Edit.cshtml", model);
    }

    // ============================================================
    // EDIT - POST
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        SubjectEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Subjects/Edit.cshtml", model);
        }

        var result =
            await _subjectService.UpdateSubjectAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Unable to update subject.");

            return View("~/Views/Subjects/Edit.cshtml", model);
        }

        TempData["SuccessMessage"] =
            "Subject updated successfully.";

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
            await _subjectService.ArchiveSubjectAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ?? "Unable to archive subject.";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Subject archived successfully.";
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
            await _subjectService.RestoreSubjectAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ?? "Unable to restore subject.";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Subject restored successfully.";
        }

        return RedirectToAction(nameof(Index));
    }
}