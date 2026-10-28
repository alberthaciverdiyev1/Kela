using FluentValidation;
using Edutor.Core.Common.Bulk;
using Edutor.Core.Common.Sorting;
using Edutor.Core.Modules.Lesson.Abstractions;
using Edutor.Core.Modules.Lesson.Requests;
using Edutor.Core.Modules.Subject.Abstractions;
using Edutor.Core.Modules.Subject.Requests;
using Edutor.Core.Modules.User.Abstractions;
using Edutor.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Mvc;
using Edutor.Admin.Infrastructure;
using Edutor.Core.Common.Requests;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class LessonController(
    ILessonService service,
    ISubjectService subjects,
    IUserService users,
    IValidator<LessonUpsertRequest> validator) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery(Name = "subject_id")] long? subjectId, CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/lesson/list.js", "~/js/pages/lesson/create.js", "~/js/pages/lesson/update.js", "~/js/pages/lesson/settings.js", "~/js/pages/lesson/sort.js" };
        ViewData["Subjects"] = await subjects.ListAsync(
            new SubjectListRequest { PerPage = 200, UserId = User.IsAdmin() ? null : User.CurrentUserId() }, ct);
        ViewData["SelectedSubjectId"] = subjectId?.ToString() ?? "";

        // Tək müəllim varsa paylaşım seçimi mənasızdır.
        var staff = await users.ListAsync(new UserListRequest
        {
            PerPage = 1,
            Type = Edutor.Core.Modules.User.Entities.UserTypes.Teacher
        }, ct);
        ViewData["MultipleTeachers"] = staff.Total > 1;

        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] LessonListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin())
        {
            // Teacher yalnız öz dərslərini görür.
            request.IsDeleted = 0;
            request.UserId = User.CurrentUserId();
        }
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, OwnerScope, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] LessonUpsertRequest request, CancellationToken ct)
    {
        // Teacher yaratdığı qeydin sahibi olur (özü görə bilsin).
        if (!User.IsAdmin()) request.UserId = User.CurrentUserId();

        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] LessonUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.UpdateAsync(id, request, OwnerScope, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, OwnerScope, ct);
        return Json(new { ok = true });
    }

    // --- Məzmun açarları (mövzu izahı, video, pdf, səs, imtahanlar) ---

    [HttpGet("{id:long}/settings")]
    public async Task<IActionResult> GetSettings(long id, CancellationToken ct)
        => Json(await service.GetSettingsAsync(id, OwnerScope, ct));

    [HttpPut("{id:long}/settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetSettings(long id, [FromBody] SetLessonSettingsRequest request, CancellationToken ct)
        => Json(await service.SetSettingsAsync(id, request, OwnerScope, ct));

    [HttpPost("sort")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sort([FromBody] SortRequest request, CancellationToken ct)
    {
        await service.ReorderAsync(request, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPut("{id:long}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(long id, [FromBody] SetActiveRequest request, CancellationToken ct)
    {
        await service.SetActiveAsync(id, request.IsActive, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
        => BulkAsync(request, service.BulkAsync, ct);

}
