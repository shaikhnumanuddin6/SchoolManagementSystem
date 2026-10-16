
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Domain.Entities;
using System.Security.Claims;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AnnouncementsController : Controller
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(
        IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    // GET: /Announcements
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var announcements = await _announcementService.GetAllAsync();
        return View(announcements);
    }

    // GET: /Announcements/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Announcement
        {
            TargetAudience = "Everyone",
            PublishAt = null,
            ExpiresAt = null
        });
    }

    // POST: /Announcements/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Announcement model)
    {
        ValidateAnnouncement(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.CreatedByUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _announcementService.CreateAsync(model);

        TempData["SuccessMessage"] =
            "Announcement created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Announcements/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var announcement = await _announcementService.GetByIdAsync(id);

        if (announcement == null)
        {
            return NotFound();
        }

        return View(announcement);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Announcement model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        ValidateAnnouncement(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await _announcementService.GetByIdAsync(id);

        if (existing == null)
        {
            return NotFound();
        }

        // Preserve server-controlled properties.
        model.CreatedAt = existing.CreatedAt;
        model.CreatedByUserId = existing.CreatedByUserId;
        model.IsPublished = existing.IsPublished;

        var updated = await _announcementService.UpdateAsync(model);

        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Announcement updated successfully.";

        return RedirectToAction(nameof(Index));
    }


    // POST: /Announcements/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _announcementService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Announcement deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    // POST: /Announcements/TogglePublish/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(int id)
    {
        var updated =
            await _announcementService.TogglePublishAsync(id);

        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Announcement publication status updated.";

        return RedirectToAction(nameof(Index));
    }

    private void ValidateAnnouncement(Announcement model)
    {
        var validAudiences = new[]
        {
            "Everyone",
            "Students",
            "Teachers"
        };

        if (!validAudiences.Contains(model.TargetAudience))
        {
            ModelState.AddModelError(
                nameof(model.TargetAudience),
                "Please select a valid target audience.");
        }

        if (model.PublishAt.HasValue &&
            model.ExpiresAt.HasValue &&
            model.ExpiresAt.Value <= model.PublishAt.Value)
        {
            ModelState.AddModelError(
                nameof(model.ExpiresAt),
                "The expiry date must be later than the publish date.");
        }

        if (model.PublishAt.HasValue &&
            model.ExpiresAt.HasValue &&
            model.PublishAt.Value.Kind != model.ExpiresAt.Value.Kind)
        {
            ModelState.AddModelError(
                nameof(model.ExpiresAt),
                "Publish and expiry dates must use the same date format.");
        }
    }
}
