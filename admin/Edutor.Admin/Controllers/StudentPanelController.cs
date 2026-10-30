using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Attendance.Abstractions;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Homework.Abstractions;
using Edutor.Core.Modules.Payments.Abstractions;
using Edutor.Core.Modules.Progress.Abstractions;
using Edutor.Core.Modules.StudentApp.Abstractions;
using Edutor.Core.Modules.Students.Abstractions;
using Edutor.Core.Modules.Students.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Şagird tətbiqinin ekranlarını admin panelindən oxumaq üçün (yalnız görünüş).</summary>
[Authorize(Policy = AuthPolicies.Admin)]
[Route("[controller]")]
public sealed class StudentPanelController(
    IStudentManagementService students,
    IStudentAppService app,
    IStudentHomeworkService homework,
    IAttendanceService attendance,
    IStudentPaymentService payments,
    IProgressService progress) : AdminControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/student-panel/index.js" };
        return View();
    }

    [HttpGet("students")]
    public async Task<IActionResult> Students(string? search, CancellationToken ct)
    {
        var result = await students.ListAsync(new StudentListRequest { Search = search, PerPage = 20 }, ct);
        return Json(result.Data.Select(s => new { id = s.Id, name = s.FullName, email = s.Email }));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Profile(long id, CancellationToken ct)
        => Json(await students.GetAsync(id, ct));

    [HttpGet("{id:long}/groups")]
    public async Task<IActionResult> Groups(long id, CancellationToken ct)
        => Json(await app.ListGroupsAsync(id, ct));

    [HttpGet("{id:long}/subjects")]
    public async Task<IActionResult> Subjects(long id, CancellationToken ct)
        => Json(await app.ListAllSubjectsAsync(id, ct));

    [HttpGet("{id:long}/subjects/{subjectId:long}/lessons")]
    public async Task<IActionResult> Lessons(long id, long subjectId, CancellationToken ct)
        => Json(await app.ListLessonsAsync(id, subjectId, ct));

    [HttpGet("{id:long}/lessons/{lessonId:long}/contents")]
    public async Task<IActionResult> Contents(long id, long lessonId, CancellationToken ct)
        => Json(await app.ListContentsAsync(id, lessonId, ct));

    [HttpGet("{id:long}/homeworks")]
    public async Task<IActionResult> Homeworks(long id, CancellationToken ct)
        => Json(await homework.ListAsync(id, ct));

    [HttpGet("{id:long}/attendance")]
    public async Task<IActionResult> Attendance(long id, CancellationToken ct)
        => Json(await attendance.GetStudentSummaryAsync(id, 20, ct));

    [HttpGet("{id:long}/payments")]
    public async Task<IActionResult> Payments(long id, CancellationToken ct)
        => Json(await payments.GetStudentOverviewAsync(id, ct));

    [HttpGet("{id:long}/progress")]
    public async Task<IActionResult> Progress(long id, CancellationToken ct)
        => Json(await progress.GetStatisticsAsync(id, ct));
}
