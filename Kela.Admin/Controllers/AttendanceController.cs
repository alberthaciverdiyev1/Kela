using FluentValidation;
using Kela.Core.Modules.Attendance.Abstractions;
using Kela.Core.Modules.Attendance.Requests;
using Kela.Core.Modules.Group.Abstractions;
using Kela.Core.Modules.Group.Requests;
using Microsoft.AspNetCore.Mvc;

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
        ViewData["Groups"] = await groups.ListAsync(new GroupListRequest { PerPage = 500 }, ct);
        return View();
    }

    [HttpGet("month")]
    public async Task<IActionResult> Month(
        [FromQuery(Name = "group_id")] long groupId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
        => Json(await attendance.GetMonthAsync(groupId, year, month, ct));

    [HttpGet("day")]
    public async Task<IActionResult> Day(
        [FromQuery(Name = "group_id")] long groupId,
        [FromQuery(Name = "date")] DateOnly date,
        CancellationToken ct)
        => Json(await attendance.GetDayAsync(groupId, date, ct));

    [HttpPost("save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] SaveAttendanceRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));
        return Json(await attendance.SaveDayAsync(request, ct));
    }

    private static object Errors(FluentValidation.Results.ValidationResult result)
        => new
        {
            errors = result.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };
}
