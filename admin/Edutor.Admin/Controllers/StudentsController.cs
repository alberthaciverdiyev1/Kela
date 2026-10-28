using Edutor.Core.Common.Bulk;
using Edutor.Core.Modules.Progress.Abstractions;
using Edutor.Core.Modules.Quiz.Abstractions;
using Edutor.Core.Modules.ScreenGuard.Abstractions;
using Edutor.Core.Modules.Students.Abstractions;
using Edutor.Core.Modules.Students.Requests;
using Microsoft.AspNetCore.Mvc;
using Edutor.Admin.Infrastructure;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class StudentsController(
    IStudentManagementService service,
    Edutor.Core.Modules.Group.Abstractions.IGroupService groups,
    IProgressService progress,
    IStudentQuizService quizzes,
    IScreenGuardService screenGuard) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/students/list.js", "~/js/pages/students/create.js",
            "~/js/pages/students/devices.js"
        };
        ViewData["Groups"] = await groups.ListAsync(
            new Edutor.Core.Modules.Group.Requests.GroupListRequest
            {
                PerPage = 500,
                UserId = User.IsAdmin() ? null : User.CurrentUserId()
            }, ct);
        return View();
    }

    private async Task<bool> CanManageAsync(long studentId, CancellationToken ct)
        => User.IsAdmin() || await service.IsInTeacherGroupsAsync(studentId, User.CurrentUserId() ?? 0, ct);

    [HttpGet("{id:long}/detail")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        if (!await CanManageAsync(id, ct)) return Forbid();

        var student = await service.GetAsync(id, ct);
        ViewData["Title"] = student.FullName;
        ViewData["Student"] = student;
        ViewData["Statistics"] = await progress.GetStatisticsAsync(id, ct);
        ViewData["Progress"] = await progress.GetProgressAsync(id, ct);
        ViewData["Results"] = await quizzes.ListResultsAsync(id, null, null, ct);
        ViewData["Violations"] = await screenGuard.ListAsync(id, 200, ct);
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] StudentListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin())
        {
            request.IsDeleted = 0;
            request.OwnerId = User.CurrentUserId();
        }
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => await CanManageAsync(id, ct) ? Json(await service.GetAsync(id, ct)) : Forbid();

    [HttpPost("{id:long}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(long id, [FromBody] SetStudentStatusRequest request, CancellationToken ct)
        => await CanManageAsync(id, ct) ? Json(await service.SetStatusAsync(id, request, ct)) : Forbid();

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        if (!await CanManageAsync(id, ct)) return Forbid();

        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

    [HttpPost("{id:long}/restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(long id, CancellationToken ct)
    {
        // Bərpa yalnız adminin əməliyyatıdır.
        if (!User.IsAdmin()) return Forbid();

        await service.RestoreAsync(id, ct);
        return Json(new { ok = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateStudentRequest request, CancellationToken ct)
    {
        // Teacher yalnız öz qrupuna şagird əlavə edə bilər.
        if (!User.IsAdmin() && request.GroupId is long groupId)
        {
            var group = await groups.GetAsync(groupId, ct: ct);
            if (group.UserId != User.CurrentUserId()) return Forbid();
        }

        try
        {
            return Json(await service.CreateAsync(request, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetStudentPasswordRequest request, CancellationToken ct)
    {
        if (!await CanManageAsync(id, ct)) return Forbid();

        try
        {
            return Json(await service.ResetPasswordAsync(id, request, ct));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:long}/devices")]
    public async Task<IActionResult> Devices(long id, CancellationToken ct)
        => await CanManageAsync(id, ct) ? Json(await service.ListDevicesAsync(id, ct)) : Forbid();

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        // Bərpa yalnız adminin əməliyyatıdır — silinmişləri yalnız o görür.
        if (request.Action == BulkActions.Restore && !User.IsAdmin()) return Forbid();

        if (!User.IsAdmin())
        {
            var teacherId = User.CurrentUserId() ?? 0;
            foreach (var id in request.Ids.Distinct())
            {
                if (!await service.IsInTeacherGroupsAsync(id, teacherId, ct)) return Forbid();
            }
        }

        await service.BulkAsync(request, ct);
        return Json(new { ok = true });
    }

    [HttpDelete("{id:long}/devices")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnbindDevice(long id, CancellationToken ct)
    {
        if (!await CanManageAsync(id, ct)) return Forbid();

        await service.UnbindDeviceAsync(id, ct);
        return Json(new { ok = true });
    }
}
