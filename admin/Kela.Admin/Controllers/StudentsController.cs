using Kela.Core.Common.Bulk;
using Kela.Core.Modules.Progress.Abstractions;
using Kela.Core.Modules.Quiz.Abstractions;
using Kela.Core.Modules.ScreenGuard.Abstractions;
using Kela.Core.Modules.Students.Abstractions;
using Kela.Core.Modules.Students.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class StudentsController(
    IStudentManagementService service,
    Kela.Core.Modules.Group.Abstractions.IGroupService groups,
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
            "~/js/pages/students/generate.js", "~/js/pages/students/devices.js"
        };
        ViewData["Groups"] = await groups.ListAsync(new Kela.Core.Modules.Group.Requests.GroupListRequest { PerPage = 500 }, ct);
        return View();
    }

    [HttpGet("{id:long}/detail")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
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
        if (!User.IsAdmin()) request.IsDeleted = 0;
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost("{id:long}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(long id, [FromBody] SetStudentStatusRequest request, CancellationToken ct)
        => Json(await service.SetStatusAsync(id, request, ct));

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

    [HttpPost("{id:long}/restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(long id, CancellationToken ct)
    {
        await service.RestoreAsync(id, ct);
        return Json(new { ok = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateStudentRequest request, CancellationToken ct)
    {
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
        try
        {
            return Json(await service.ResetPasswordAsync(id, request, ct));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate([FromBody] GenerateStudentsRequest request, CancellationToken ct)
        => Json(await service.GenerateAsync(request, ct));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] StudentListRequest request, CancellationToken ct)
    {
        var bytes = await service.ExportXlsxAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"students-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet("{id:long}/devices")]
    public async Task<IActionResult> Devices(long id, CancellationToken ct)
        => Json(await service.ListDevicesAsync(id, ct));

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        // Bərpa yalnız adminin əməliyyatıdır — silinmişləri yalnız o görür.
        if (request.Action == BulkActions.Restore && !User.IsAdmin()) return Forbid();
        await service.BulkAsync(request, ct);
        return Json(new { ok = true });
    }

    [HttpDelete("{id:long}/devices")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnbindDevice(long id, CancellationToken ct)
    {
        await service.UnbindDeviceAsync(id, ct);
        return Json(new { ok = true });
    }
}
