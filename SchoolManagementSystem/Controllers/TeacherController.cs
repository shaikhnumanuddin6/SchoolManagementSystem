
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ITeacherDashboardService _dashboardService;
    private readonly IAnnouncementService _announcementService;

    public TeacherController(
        ITeacherDashboardService dashboardService,
        IAnnouncementService announcementService)
    {
        _dashboardService = dashboardService;
        _announcementService = announcementService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var applicationUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(applicationUserId))
        {
            return Challenge();
        }

        var model =
            await _dashboardService.GetDashboardAsync(applicationUserId);

        if (model == null)
        {
            return NotFound("Teacher profile was not found.");
        }

        return View("~/Views/Teachers/Dashboard.cshtml", model);
    }

    [HttpGet]
    public async Task<IActionResult> Announcements()
    {
        var announcements =
            await _announcementService.GetPublishedAsync("Teachers");

        return View("~/Views/Teachers/Announcements.cshtml", announcements);
    }
}
