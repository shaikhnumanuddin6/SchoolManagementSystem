
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AIDashboardController : Controller
{
    private readonly IAIService _aiService;
    private readonly ILogger<AIDashboardController> _logger;

    public AIDashboardController(
        IAIService aiService,
        ILogger<AIDashboardController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            TempData["AIError"] = "Please enter a question.";
            return RedirectToAction(nameof(Index));
        }

        if (prompt.Length > 4000)
        {
            TempData["AIError"] =
                "Please keep your question under 4,000 characters.";

            return RedirectToAction(nameof(Index));
        }

        try
        {
            var answer = await _aiService.GetResponseAsync(prompt);

            TempData["AIAnswer"] = answer;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Groq API request failed.");

            // Display the detailed error returned by the AI service.
            TempData["AIError"] = ex.Message;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Groq API request timed out.");

            TempData["AIError"] =
                "The AI request timed out. Please try again.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI dashboard request failed.");

            TempData["AIError"] =
                "Something went wrong while generating the answer. " +
                "Check the Visual Studio Output window for details.";
        }

        return RedirectToAction(nameof(Index));
    }
}
