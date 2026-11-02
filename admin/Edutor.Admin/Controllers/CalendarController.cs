using FluentValidation;
using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Calendar.Abstractions;
using Edutor.Core.Modules.Calendar.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class CalendarController(
    ICalendarService service,
    IValidator<TeacherEventUpsertRequest> validator) : AdminControllerBase
{
    private long UserId => User.CurrentUserId() ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/calendar/index.js" };
        return View();
    }

    [HttpGet("events")]
    public async Task<IActionResult> Events(
        [FromQuery(Name = "from")] DateTimeOffset from,
        [FromQuery(Name = "to")] DateTimeOffset to,
        CancellationToken ct)
        => Json(await service.GetRangeAsync(UserId, User.IsAdmin(), from, to, ct));

    [HttpPost("events")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] TeacherEventUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateEventAsync(UserId, request, ct));
    }

    [HttpPut("events/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromBody] TeacherEventUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.UpdateEventAsync(UserId, id, request, ct));
    }

    [HttpDelete("events/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteEventAsync(UserId, id, ct);
        return Json(new { ok = true });
    }
}
