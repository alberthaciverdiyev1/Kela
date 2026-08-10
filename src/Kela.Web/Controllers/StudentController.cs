using Kela.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Web.Controllers;

[Authorize(Roles = AppConstants.RoleStudent)]
public sealed class StudentController(IApiClient api, IConfiguration config) : Controller
{
    public IActionResult Dashboard() => View();

    public IActionResult Courses()
    {
        ViewData["TitleKey"] = "nav.myCourses";
        return View("~/Views/Shared/ComingSoon.cshtml");
    }

    [HttpGet("student/lessons/{contentId:int}")]
    public async Task<IActionResult> Lesson(int contentId, CancellationToken ct)
    {
        var result = await api.GetLessonAsync(contentId, ct);
        if (!result.Success || result.Data is null)
        {
            return NotFound();
        }

        var lesson = result.Data;
        var apiBase = (config["Api:BaseUrl"] ?? "https://localhost:7047").TrimEnd('/');
        return View(new Kela.Web.Models.Lessons.LessonEditorViewModel(
            lesson,
            $"{apiBase}/api/lessons/{contentId}/stream"));
    }
}
