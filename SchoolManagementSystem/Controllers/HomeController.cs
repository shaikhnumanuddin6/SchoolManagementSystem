using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Models;
using System.Diagnostics;

namespace SchoolManagementSystem.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDashboardService _dashboardService;

    public HomeController(
        ILogger<HomeController> logger,
        IDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    // ============================================================
    // DASHBOARD
    // ============================================================

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Student"))
        {
            return RedirectToAction("Dashboard", "Student");
        }

        var dashboard = await _dashboardService.GetDashboardAsync();

        return View(dashboard);
    }

    // ============================================================
    // PRIVACY
    // ============================================================

    public IActionResult Privacy()
    {
        return View();
    }

    // ============================================================
    // ERROR
    // ============================================================

    [AllowAnonymous]
    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
    }
}
