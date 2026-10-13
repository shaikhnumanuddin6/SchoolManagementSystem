
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.ViewModels.Timetable;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class TimetableController : Controller
{
    private readonly ITimetableService _timetableService;

    public TimetableController(ITimetableService timetableService)
    {
        _timetableService = timetableService;
    }

    // GET: /Timetable
    [HttpGet]
    public async Task<IActionResult> Index(int? classId, int? dayOfWeek)
    {
        if (dayOfWeek.HasValue &&
            (dayOfWeek.Value < 1 || dayOfWeek.Value > 7))
        {
            dayOfWeek = null;
        }

        var model = await _timetableService.GetListAsync(
            classId,
            dayOfWeek);

        return View(model);
    }

    // GET: /Timetable/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await _timetableService.GetFormViewModelAsync();

        return View(model);
    }

    // POST: /Timetable/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TimetableFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel =
                await _timetableService.GetFormViewModelAsync();

            CopyFormValues(model, invalidModel);

            return View(invalidModel);
        }

        var result = await _timetableService.SaveAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Unable to save the timetable entry.");

            var invalidModel =
                await _timetableService.GetFormViewModelAsync();

            CopyFormValues(model, invalidModel);

            return View(invalidModel);
        }

        TempData["SuccessMessage"] =
            "Timetable entry created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Timetable/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var model =
                await _timetableService.GetFormViewModelAsync(id);

            return View(model);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // POST: /Timetable/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        TimetableFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var invalidModel =
                await _timetableService.GetFormViewModelAsync(id);

            CopyFormValues(model, invalidModel);

            return View(invalidModel);
        }

        var result = await _timetableService.SaveAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Unable to update the timetable entry.");

            var invalidModel =
                await _timetableService.GetFormViewModelAsync(id);

            CopyFormValues(model, invalidModel);

            return View(invalidModel);
        }

        TempData["SuccessMessage"] =
            "Timetable entry updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // POST: /Timetable/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _timetableService.DeleteAsync(id);

        if (deleted)
        {
            TempData["SuccessMessage"] =
                "Timetable entry deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] =
                "Timetable entry was not found.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static void CopyFormValues(
        TimetableFormViewModel source,
        TimetableFormViewModel destination)
    {
        destination.Id = source.Id;
        destination.ClassId = source.ClassId;
        destination.SubjectId = source.SubjectId;
        destination.TeacherId = source.TeacherId;
        destination.DayOfWeek = source.DayOfWeek;
        destination.StartTime = source.StartTime;
        destination.EndTime = source.EndTime;
        destination.RoomNumber = source.RoomNumber;
    }
}
