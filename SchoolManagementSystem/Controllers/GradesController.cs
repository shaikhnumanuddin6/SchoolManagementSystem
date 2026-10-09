using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Grades;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class GradesController : Controller
{
    private readonly IGradeService _gradeService;

    public GradesController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    // ============================================================
    // GET: /Grades
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] GradeListViewModel filter)
    {
        var model = await _gradeService.GetAllAsync(filter);
        return View(model);
    }

    // ============================================================
    // GET: /Grades/Details/5
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _gradeService.GetDetailsAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    // ============================================================
    // GET: /Grades/Create
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await _gradeService.GetCreateViewModelAsync();
        return View(model);
    }

    // ============================================================
    // POST: /Grades/Create
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        GradeCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await RefreshCreateLookupsAsync(model);
            return View(model);
        }

        var result = await _gradeService.CreateAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Unable to create the grade.");

            await RefreshCreateLookupsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Grade created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // GET: /Grades/Edit/5
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _gradeService.GetEditViewModelAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    // ============================================================
    // POST: /Grades/Edit/5
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        GradeEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            if (!await RefreshEditLookupsAsync(model))
            {
                return NotFound();
            }

            return View(model);
        }

        var result = await _gradeService.UpdateAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Unable to update the grade.");

            if (!await RefreshEditLookupsAsync(model))
            {
                return NotFound();
            }

            return View(model);
        }

        TempData["SuccessMessage"] = "Grade updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // ============================================================
    // RELOAD CREATE DROPDOWNS AFTER VALIDATION ERRORS
    // ============================================================

    private async Task RefreshCreateLookupsAsync(
        GradeCreateViewModel model)
    {
        var freshModel =
            await _gradeService.GetCreateViewModelAsync();

        model.Students = freshModel.Students;
        model.Classes = freshModel.Classes;
        model.Subjects = freshModel.Subjects;

        foreach (var item in model.Students)
        {
            item.Selected =
                item.Value == model.StudentId?.ToString();
        }

        foreach (var item in model.Classes)
        {
            item.Selected =
                item.Value == model.ClassId?.ToString();
        }

        foreach (var item in model.Subjects)
        {
            item.Selected =
                item.Value == model.SubjectId?.ToString();
        }
    }

    // ============================================================
    // RELOAD EDIT DROPDOWNS AFTER VALIDATION ERRORS
    // ============================================================

    private async Task<bool> RefreshEditLookupsAsync(
        GradeEditViewModel model)
    {
        var freshModel =
            await _gradeService.GetEditViewModelAsync(model.Id);

        if (freshModel == null)
        {
            return false;
        }

        model.Students = freshModel.Students;
        model.Classes = freshModel.Classes;
        model.Subjects = freshModel.Subjects;

        foreach (var item in model.Students)
        {
            item.Selected =
                item.Value == model.StudentId?.ToString();
        }

        foreach (var item in model.Classes)
        {
            item.Selected =
                item.Value == model.ClassId?.ToString();
        }

        foreach (var item in model.Subjects)
        {
            item.Selected =
                item.Value == model.SubjectId?.ToString();
        }

        return true;
    }
}