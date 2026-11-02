using FluentValidation;
using Edutor.Admin.Infrastructure;
using Edutor.Core.Common.Notes;
using Edutor.Core.Modules.TeacherNote.Abstractions;
using Edutor.Core.Modules.TeacherNote.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class TeacherNoteController(
    ITeacherNoteService service,
    IValidator<TeacherNoteUpsertRequest> validator) : AdminControllerBase
{
    private long UserId => User.CurrentUserId() ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/teacher-note/index.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] TeacherNoteListRequest request, CancellationToken ct)
        => Json(await service.ListAsync(UserId, request, ct));

    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] string? search, CancellationToken ct)
        => Json(await service.ListShareTargetsAsync(UserId, search, ct));

    [HttpGet("contents")]
    public async Task<IActionResult> Contents([FromQuery] string? search, CancellationToken ct)
        => Json(await service.ListContentOptionsAsync(search, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(UserId, id, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] TeacherNoteUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(UserId, request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] TeacherNoteUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.UpdateAsync(UserId, id, request, ct));
    }

    [HttpPut("{id:long}/pin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pin(long id, [FromBody] NotePinRequest request, CancellationToken ct)
        => Json(await service.SetPinnedAsync(UserId, id, request.Pinned, ct));

    [HttpPut("{id:long}/color")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Color(long id, [FromBody] NoteColorRequest request, CancellationToken ct)
        => Json(await service.SetColorAsync(UserId, id, request.Color, ct));

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(UserId, id, ct);
        return Json(new { ok = true });
    }
}
