using FluentValidation;
using LanguageLMS.Core.Modules.Lesson.Abstractions;
using LanguageLMS.Core.Modules.Lesson.Requests;
using LanguageLMS.Core.Modules.Subject.Abstractions;
using LanguageLMS.Core.Modules.Subject.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Mvc.Controllers;

public sealed class LessonController(
    ILessonService service,
    ISubjectService subjects,
    IValidator<LessonUpsertRequest> validator) : Controller
{
    public async Task<IActionResult> Index(LessonListRequest request, CancellationToken ct)
    {
        ViewData["Title"] = "Dərslər";
        ViewData["Scripts"] = new[] { "~/pages/lesson/list.js", "~/pages/lesson/form.js" };
        var lessons = await service.ListAsync(request, ct);
        var subjectList = await subjects.ListAsync(new SubjectListRequest { PerPage = 200 }, ct);
        return View((lessons, subjectList));
    }

    [HttpGet]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] LessonUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] LessonUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.UpdateAsync(id, request, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

    private static object Errors(FluentValidation.Results.ValidationResult result)
        => new
        {
            errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };
}
