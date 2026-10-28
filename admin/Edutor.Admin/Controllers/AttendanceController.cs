using FluentValidation;
using Edutor.Core.Modules.Attendance.Abstractions;
using Edutor.Core.Modules.Attendance.Requests;
using Edutor.Core.Modules.Group.Abstractions;
using Edutor.Core.Modules.Group.Requests;
using Microsoft.AspNetCore.Mvc;
using Edutor.Admin.Infrastructure;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class AttendanceController(
    IAttendanceService attendance,
    IGroupService groups,
    IValidator<SaveAttendanceRequest> validator) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/attendance/index.js" };
        // Teacher yalnız öz qruplarını görür; admin/superadmin hamısını.
        ViewData["Groups"] = await groups.ListAsync(
            new GroupListRequest { PerPage = 500, UserId = User.IsAdmin() ? null : User.CurrentUserId() }, ct);
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
        if (!result.IsValid) return BadRequest(Invalid(result));
        return Json(await attendance.SaveDayAsync(request, ct));
    }

    /// <summary>Teacher yalnız öz qrupunun yoxlamasını görə/dəyişə bilər.</summary>
    private async Task<bool> CanReadAsync(long groupId, CancellationToken ct)
    {
        if (User.IsAdmin()) return true;

        try
        {
            var group = await groups.GetAsync(groupId, ct: ct);
            return group.UserId == User.CurrentUserId();
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

}
