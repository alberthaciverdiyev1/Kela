using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Kela.Web.Helpers;
using Kela.Web.Localization;
using Kela.Web.Models.Attendances;
using Kela.Web.Models.Students;
using Kela.Web.Models.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Web.Controllers;

[Authorize(Roles = AppConstants.RoleTeacher)]
public sealed partial class TeacherController(IApiClient api, Localizer L) : Controller
{
    private const int PageSize = 10;

    private int UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public IActionResult Dashboard() => View();

    public IActionResult Students() => View("Students/Students");

    [HttpGet("teacher/students/table")]
    public async Task<IActionResult> StudentsTable(CancellationToken ct, int page = 1, string? search = null)
    {
        var result = await api.GetStudentsPageAsync(page, PageSize, search, ct);
        return Json(result.Data ?? new PaginatedResult<StudentResponse>([], page, PageSize, 0));
    }

    [HttpPost("teacher/students/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStudent(CreateStudentViewModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
        {
            ModelState.AddModelError(nameof(model.FirstName), L.T("students.reqFirstName"));
        }

        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), L.T("students.reqPhone"));
        }

        if (!string.IsNullOrWhiteSpace(model.Email) && !EmailPattern().IsMatch(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), L.T("students.invalidEmail"));
        }

        if (!ModelState.IsValid)
        {
            return ValidationErrors();
        }

        var created = await api.CreateStudentAsync(new CreateStudentRequest(
            model.FirstName!.Trim(),
            NullIfBlank(model.LastName),
            model.PhoneNumber!.Trim(),
            NullIfBlank(model.Email),
            null,
            null), ct);

        if (!created.Success)
        {
            foreach (var error in created.Errors ?? [])
            {
                ModelState.AddModelError("", error);
            }

            if (!string.IsNullOrWhiteSpace(created.Message))
            {
                ModelState.AddModelError("", created.Message);
            }

            return ValidationErrors();
        }

        Response.StatusCode = StatusCodes.Status201Created;
        return Json(created.Data!);
    }

    [HttpDelete("teacher/students/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudent(int id, CancellationToken ct, int page = 1, string? search = null)
    {
        await api.DeleteStudentAsync(id, ct);

        var result = await api.GetStudentsPageAsync(page, PageSize, search, ct);
        if ((result.Data?.Items.Count ?? 0) == 0 && page > 1)
        {
            result = await api.GetStudentsPageAsync(page - 1, PageSize, search, ct);
            page--;
        }

        return Json(result.Data ?? new PaginatedResult<StudentResponse>([], page, PageSize, 0));
    }

    [HttpGet("teacher/workspaces")]
    public IActionResult Index() => View("Workspaces/Workspaces");

    [HttpGet("teacher/workspaces/table")]
    public async Task<IActionResult> WorkspacesTable(CancellationToken ct, int page = 1)
    {
        var result = await api.GetWorkspacesPageAsync(UserId, page, PageSize, ct);
        return Json(result.Data ?? new PaginatedResult<WorkspaceResponse>([], page, PageSize, 0));
    }

    [HttpPost("teacher/workspaces/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWorkspace(CreateWorkspaceViewModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), L.T("workspaces.reqName"));
        }

        if (!ModelState.IsValid)
        {
            return ValidationErrors();
        }

        var created = await api.CreateWorkspaceAsync(new CreateWorkspaceRequest(model.Name!.Trim(), UserId), ct);

        if (!created.Success || created.Data is null)
        {
            foreach (var error in created.Errors ?? [])
            {
                ModelState.AddModelError("", error);
            }

            if (!string.IsNullOrWhiteSpace(created.Message))
            {
                ModelState.AddModelError("", created.Message);
            }

            return ValidationErrors();
        }

        return Json(new { redirect = $"/teacher/workspaces/{created.Data.Id}" });
    }

    [HttpGet("teacher/workspaces/{id:int}")]
    public async Task<IActionResult> WorkspaceDetail(int id, CancellationToken ct)
    {
        var workspace = await api.GetWorkspaceAsync(id, ct);
        if (!workspace.Success || workspace.Data is null)
        {
            return NotFound();
        }

        var data = workspace.Data;
        return View("Workspaces/WorkspaceDetail", new WorkspaceShellViewModel(data.Id, data.Name, data.StudentCount, data.CreatedAt));
    }

    [HttpGet("teacher/workspaces/{id:int}/data")]
    public async Task<IActionResult> WorkspaceData(int id, CancellationToken ct)
    {
        var data = await WorkspaceDataAsync(id, ct);
        if (data is null)
        {
            return NotFound();
        }

        return Json(data);
    }

    [HttpPost("teacher/workspaces/{id:int}/rename")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameWorkspace(int id, CreateWorkspaceViewModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), L.T("workspaces.reqName"));
        }

        if (!ModelState.IsValid)
        {
            return ValidationErrors();
        }

        var result = await api.UpdateWorkspaceAsync(id, new UpdateWorkspaceRequest(model.Name!.Trim()), ct);

        if (!result.Success)
        {
            foreach (var error in result.Errors ?? [])
            {
                ModelState.AddModelError("", error);
            }

            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                ModelState.AddModelError("", result.Message);
            }

            return ValidationErrors();
        }

        return Json(new { redirect = $"/teacher/workspaces/{id}" });
    }

    [HttpDelete("teacher/workspaces/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWorkspace(int id, CancellationToken ct, int page = 1)
    {
        await api.DeleteWorkspaceAsync(id, ct);

        var result = await api.GetWorkspacesPageAsync(UserId, page, PageSize, ct);
        if ((result.Data?.Items.Count ?? 0) == 0 && page > 1)
        {
            result = await api.GetWorkspacesPageAsync(UserId, page - 1, PageSize, ct);
            page--;
        }

        return Json(result.Data ?? new PaginatedResult<WorkspaceResponse>([], page, PageSize, 0));
    }

    [HttpPost("teacher/workspaces/{id:int}/students")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWorkspaceStudents(int id, AddWorkspaceStudentsViewModel model, CancellationToken ct)
    {
        await api.AddStudentsAsync(id, new AddStudentsRequest(model.StudentIds), ct);
        var data = await WorkspaceDataAsync(id, ct);
        return data is null ? NotFound() : Json(data);
    }

    [HttpDelete("teacher/workspaces/{id:int}/students/{studentId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveWorkspaceStudent(int id, int studentId, CancellationToken ct)
    {
        await api.RemoveStudentAsync(id, studentId, ct);
        var data = await WorkspaceDataAsync(id, ct);
        return data is null ? NotFound() : Json(data);
    }

    [HttpGet("teacher/attendance")]
    public async Task<IActionResult> Attendance(int? workspaceId, string? month, CancellationToken ct)
    {
        var (year, monthNumber) = ParseMonth(month);
        var workspacesResult = await api.GetWorkspacesPageAsync(UserId, 1, 1000, ct);
        var workspaces = (workspacesResult.Data?.Items ?? []).Select(w => new WorkspaceOption(w.Id, w.Name)).ToList();

        var selected = workspaceId is > 0 ? workspaceId.Value : 0;
        if (!workspaces.Any(w => w.Id == selected))
        {
            selected = 0;
        }

        var name = selected == 0 ? "" : workspaces.First(w => w.Id == selected).Name;
        return View("Attendance/Index", new AttendancePageViewModel(selected, name, year, monthNumber, workspaces));
    }

    [HttpGet("teacher/attendance/data")]
    public async Task<IActionResult> AttendanceData(int? workspaceId, string? month, CancellationToken ct)
    {
        if (workspaceId is not > 0)
        {
            return BadRequest();
        }

        var (year, monthNumber) = ParseMonth(month);
        var data = await api.GetAttendanceMonthAsync(workspaceId.Value, year, monthNumber, ct);
        if (!data.Success || data.Data is null)
        {
            return NotFound();
        }

        return Json(data.Data);
    }

    [HttpPut("teacher/attendance")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAttendance([FromBody] SetAttendanceRequest model, CancellationToken ct)
    {
        if (model is null || model.WorkspaceId <= 0 || model.StudentId <= 0 || model.Date == default || model.Status is < 0 or > 4)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return Json(new { success = false, message = L.T("attendance.invalid") });
        }

        var result = await api.SetAttendanceMarkAsync(model.WorkspaceId, model.Date, model.StudentId, model.Status, ct);

        if (!result.Success)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return Json(new { success = false, message = result.Message ?? L.T("attendance.saveError") });
        }

        return Json(new { success = true });
    }

    public IActionResult Settings()
    {
        ViewData["TitleKey"] = "nav.settings";
        return View("~/Views/Shared/ComingSoon.cshtml");
    }

    private static (int Year, int Month) ParseMonth(string? month)
    {
        if (!string.IsNullOrWhiteSpace(month)
            && DateOnly.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return (parsed.Year, parsed.Month);
        }

        var now = DateTime.Now;
        return (now.Year, now.Month);
    }

    private IActionResult ValidationErrors()
    {
        Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        var errors = ModelState
            .SelectMany(kv => kv.Value?.Errors.Select(e => (kv.Key, e.ErrorMessage)) ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => string.Join(" ", g.Select(x => x.ErrorMessage)));
        return Json(new { success = false, errors });
    }

    private async Task<WorkspaceDataResponse?> WorkspaceDataAsync(int id, CancellationToken ct)
    {
        var workspace = await api.GetWorkspaceAsync(id, ct);
        if (!workspace.Success || workspace.Data is null)
        {
            return null;
        }

        var students = await api.GetStudentsPageAsync(1, 1000, ct: ct);
        var inWorkspace = workspace.Data.Students.Select(s => s.Id).ToHashSet();
        var available = (students.Data?.Items ?? []).Where(s => !inWorkspace.Contains(s.UserId)).ToList();

        return new WorkspaceDataResponse(workspace.Data, available);
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailPattern();
}
