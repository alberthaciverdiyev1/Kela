using FluentValidation;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Sorting;
using Kela.Core.Modules.Lesson.Abstractions;
using Kela.Core.Modules.Lesson.Requests;
using Kela.Core.Modules.Subject.Abstractions;
using Kela.Core.Modules.Subject.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class LessonController(
    ILessonService service,
    ISubjectService subjects,
    IValidator<LessonUpsertRequest> validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery(Name = "subject_id")] long? subjectId, CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/lesson/list.js", "~/js/pages/lesson/create.js", "~/js/pages/lesson/update.js", "~/js/pages/lesson/settings.js", "~/js/pages/lesson/sort.js" };
        ViewData["Subjects"] = await subjects.ListAsync(new SubjectListRequest { PerPage = 200 }, ct);
        ViewData["SelectedSubjectId"] = subjectId?.ToString() ?? "";
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] LessonListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin()) request.IsDeleted = 0;
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
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

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] LessonUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
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

    // --- Məzmun açarları (mövzu izahı, video, pdf, səs, imtahanlar) ---

    [HttpGet("{id:long}/settings")]
    public async Task<IActionResult> GetSettings(long id, CancellationToken ct)
        => Json(await service.GetSettingsAsync(id, ct));

    [HttpPut("{id:long}/settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetSettings(long id, [FromBody] SetLessonSettingsRequest request, CancellationToken ct)
        => Json(await service.SetSettingsAsync(id, request, ct));

    [HttpPost("sort")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sort([FromBody] SortRequest request, CancellationToken ct)
    {
        await service.ReorderAsync(request, ct);
        return Json(new { ok = true });
    }

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        // Bərpa yalnız adminin əməliyyatıdır — silinmişləri yalnız o görür.
        if (request.Action == BulkActions.Restore && !User.IsAdmin()) return Forbid();
        await service.BulkAsync(request, ct);
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
