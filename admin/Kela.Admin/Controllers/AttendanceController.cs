using FluentValidation;
using Kela.Core.Modules.Attendance.Abstractions;
using Kela.Core.Modules.Attendance.Requests;
using Kela.Core.Modules.Group.Abstractions;
using Kela.Core.Modules.Group.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class AttendanceController(
    IAttendanceService attendance,
    IGroupService groups,
    IValidator<SaveAttendanceRequest> validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/attendance/index.js" };
        // Teacher yalnız öz qruplarını görür; admin/superadmin hamısını.
        ViewData["Groups"] = await groups.ListAsync(
            new GroupListRequest { PerPage = 500, UserId = User.IsAdmin() ? null : CurrentUserId() }, ct);
        return View();
    }

    [HttpGet("month")]
    public async Task<IActionResult> Month(
        [FromQuery(Name = "group_id")] long groupId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
    {
        if (!await CanReadAsync(groupId, ct)) return Forbid();
        return Json(await attendance.GetMonthAsync(groupId, year, month, ct));
    }

    [HttpGet("day")]
    public async Task<IActionResult> Day(
        [FromQuery(Name = "group_id")] long groupId,
        [FromQuery(Name = "date")] DateOnly date,
        CancellationToken ct)
    {
        if (!await CanReadAsync(groupId, ct)) return Forbid();
        return Json(await attendance.GetDayAsync(groupId, date, ct));
    }

    [HttpPost("save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] SaveAttendanceRequest request, CancellationToken ct)
    {
        if (!await CanReadAsync(request.GroupId, ct)) return Forbid();

        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));
        return Json(await attendance.SaveDayAsync(request, ct));
    }

    /// <summary>Teacher yalnız öz qrupunun yoxlamasını görə/dəyişə bilər.</summary>
    private async Task<bool> CanReadAsync(long groupId, CancellationToken ct)
    {
        if (User.IsAdmin()) return true;

        try
        {
            var group = await groups.GetAsync(groupId, ct);
            return group.UserId == CurrentUserId();
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    private long? CurrentUserId() =>
        long.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    private static object Errors(FluentValidation.Results.ValidationResult result)
        => new
        {
            errors = result.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };
}
