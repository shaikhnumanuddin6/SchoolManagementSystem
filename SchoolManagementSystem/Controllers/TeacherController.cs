using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.Services;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ITeacherDashboardService _dashboardService;
    public TeacherController(
        ITeacherDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var applicationUserId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(applicationUserId))
        {
            return Challenge();
        }

        var model =
            await _dashboardService
                .GetDashboardAsync(applicationUserId);

        if (model == null)
        {
            return NotFound(
                "Teacher profile was not found.");
        }

        return View(
            "~/Views/Teachers/Dashboard.cshtml",
            model);
    }
}