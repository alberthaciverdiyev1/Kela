using FluentValidation;
using LanguageLMS.Core.Modules.Lesson.Abstractions;
using LanguageLMS.Core.Modules.Lesson.Requests;
using LanguageLMS.Core.Modules.LessonContent.Abstractions;
using LanguageLMS.Core.Modules.LessonContent.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Mvc.Controllers;

[Route("[controller]")]
public sealed class LessonContentController(
    ILessonContentService service,
    ILessonService lessons,
    IValidator<LessonContentCreateRequest> createValidator,
    IValidator<LessonContentUpdateRequest> updateValidator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Materiallar";
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/lesson-content/list.js",
            "~/js/pages/lesson-content/create.js",
            "~/js/pages/lesson-content/update.js"
        };
        ViewData["Lessons"] = await lessons.ListAsync(new LessonListRequest { PerPage = 200 }, ct);
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] LessonContentListRequest request, CancellationToken ct)
        => Json(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost("upload-url")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadUrl([FromForm] LessonContentUploadUrlRequest request, CancellationToken ct)
    {
        try
        {
            return Json(await service.CreateUploadUrlAsync(request, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] LessonContentCreateRequest request, CancellationToken ct)
    {
        var result = await createValidator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] LessonContentUpdateRequest request, CancellationToken ct)
    {
        var result = await updateValidator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:long}")]
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
