using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Students;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class StudentsController : Controller
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    // ============================================================
    // STUDENT LIST
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        int? classId,
        string? status)
    {
        var model = await _studentService.GetStudentsAsync(
            searchTerm,
            classId,
            status);

        return View(
            "~/Views/Students/Index.cshtml",
            model);
    }

    // ============================================================
    // CREATE STUDENT
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model =
            await _studentService.GetCreateViewModelAsync();

        return View(
            "~/Views/Student/Create.cshtml",
            model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        StudentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var createModel =
                await _studentService.GetCreateViewModelAsync();

            model.Classes = createModel.Classes;

            return View(
                "~/Views/Student/Create.cshtml",
                model);
        }

        var result =
            await _studentService.CreateStudentAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ??
                "Unable to create student.");

            var createModel =
                await _studentService.GetCreateViewModelAsync();

            model.Classes = createModel.Classes;

            return View(
                "~/Views/Student/Create.cshtml",
                model);
        }

        TempData["SuccessMessage"] =
            "Student account was created successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = result.StudentId });
    }

    // ============================================================
    // STUDENT DETAILS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model =
            await _studentService.GetDetailsAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    // ============================================================
    // EDIT STUDENT
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model =
            await _studentService.GetEditViewModelAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        StudentEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var existingModel =
                await _studentService.GetEditViewModelAsync(id);

            model.Classes =
                existingModel?.Classes ?? new();

            return View(model);
        }

        var result =
            await _studentService.UpdateStudentAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ??
                "Unable to update student.");

            var existingModel =
                await _studentService.GetEditViewModelAsync(id);

            model.Classes =
                existingModel?.Classes ?? new();

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Student information was updated successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.Id });
    }

    // ============================================================
    // ARCHIVE STUDENT
    // ============================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        var result =
            await _studentService.ArchiveStudentAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] =
                result.ErrorMessage ??
                "Unable to archive student.";

            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] =
            "Student was archived successfully.";

        return RedirectToAction(nameof(Index));
    }
}