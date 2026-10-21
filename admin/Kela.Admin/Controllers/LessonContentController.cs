using FluentValidation;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Sorting;
using Kela.Core.Modules.Lesson.Abstractions;
using Kela.Core.Modules.Lesson.Requests;
using Kela.Core.Modules.LessonContent.Abstractions;
using Kela.Core.Modules.LessonContent.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;
using Kela.Core.Common.Requests;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class LessonContentController(
    ILessonContentService service,
    ILessonService lessons,
    IValidator<LessonContentCreateRequest> createValidator,
    IValidator<LessonContentUpdateRequest> updateValidator) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery(Name = "lesson_id")] long? lessonId, CancellationToken ct)
    {
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/lesson-content/list.js",
            "~/js/pages/lesson-content/create.js",
            "~/js/pages/lesson-content/update.js",
            "~/js/pages/lesson-content/sort.js"
        };
        ViewData["Lessons"] = await lessons.ListAsync(
            new LessonListRequest { PerPage = 200, UserId = User.IsAdmin() ? null : User.CurrentUserId() }, ct);
        ViewData["SelectedLessonId"] = lessonId?.ToString() ?? "";
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] LessonContentListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin())
        {
            // Teacher yalnız öz yaratdığı materialları və admin materiallarını görür.
            request.IsDeleted = 0;
            request.UserId = User.CurrentUserId();
        }
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, OwnerScope, ct));

    [HttpPost("upload-url")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadUrl([FromForm] LessonContentUploadUrlRequest request, CancellationToken ct)
    {
        try
        {
            return Json(await service.CreateUploadUrlAsync(request, OwnerScope, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ---- Böyük fayllar üçün çoxhissəli yükləmə ----
    // Nazik presigned PUT bir neçə yüz MB-a qədər yararlıdır; GB-larla video üçün
    // müştəri faylı hissələrə bölür, hər hissəni ayrıca göndərir və sonra birləşdiririk.

    [HttpPost("upload/multipart/start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MultipartStart([FromForm] LessonContentUploadUrlRequest request, CancellationToken ct)
    {
        var ticket = await service.StartMultipartAsync(request, OwnerScope, ct);
        return ticket is null
            ? BadRequest(new { error = "Multipart upload is not supported by the current storage provider" })
            : Json(ticket);
    }

    [HttpPost("upload/multipart/parts")]
    [ValidateAntiForgeryToken]
    public IActionResult MultipartParts([FromBody] MultipartPartsRequest request)
    {
        try
        {
            return Json(service.SignParts(request));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("upload/multipart/complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MultipartComplete([FromBody] MultipartCompleteRequest request, CancellationToken ct)
    {
        try
        {
            await service.CompleteMultipartAsync(request, ct);
            return Json(new { ok = true, path = request.Path });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("upload/multipart/abort")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MultipartAbort([FromBody] MultipartAbortRequest request, CancellationToken ct)
    {
        await service.AbortMultipartAsync(request, ct);
        return Json(new { ok = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] LessonContentCreateRequest request, CancellationToken ct)
    {
        // Teacher yaratdığı materialın sahibi olur; admin materialı sahibsizdir.
        if (!User.IsAdmin()) request.UserId = User.CurrentUserId();

        var result = await createValidator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(request, ct));
    }

    /// <summary>Fon rejimində yüklənmiş faylı materiala bağlayır (yükləmə bitəndə çağırılır).</summary>
    [HttpPost("{id:long}/file")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AttachFile(long id, [FromForm] LessonContentAttachRequest request, CancellationToken ct)
    {
        try
        {
            return Json(await service.AttachFileAsync(id, request, OwnerScope, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] LessonContentUpdateRequest request, CancellationToken ct)
    {
        var result = await updateValidator.ValidateAsync(request, ct);
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
