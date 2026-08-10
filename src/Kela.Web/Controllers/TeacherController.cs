using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Kela.Web.Helpers;
using Kela.Web.Localization;
using Kela.Web.Models.Attendances;
using Kela.Web.Models.Settings;
using Kela.Web.Models.Students;
using Kela.Web.Models.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Web.Controllers;

[Authorize(Roles = AppConstants.RoleTeacher)]
public sealed partial class TeacherController(IApiClient api, Localizer L) : Controller
{
    private int UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private bool IsAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    public IActionResult Dashboard() => View();

    public IActionResult Students() => View("Students/Students");

    [HttpGet("teacher/students/table")]
    public async Task<IActionResult> StudentsTable(CancellationToken ct, int page = 1, string? search = null)
    {
        var result = await api.GetStudentsPageAsync(page, search, ct);
        return Json(result.Data ?? new PaginatedResult<StudentResponse>([], page, 20, 0));
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

        var result = await api.GetStudentsPageAsync(page, search, ct);
        if ((result.Data?.Items.Count ?? 0) == 0 && page > 1)
        {
            result = await api.GetStudentsPageAsync(page - 1, search, ct);
            page--;
        }

        return Json(result.Data ?? new PaginatedResult<StudentResponse>([], page, 20, 0));
    }

    [HttpGet("teacher/workspaces")]
    public IActionResult Index() => View("Workspaces/Workspaces");

    [HttpGet("teacher/workspaces/table")]
    public async Task<IActionResult> WorkspacesTable(CancellationToken ct, int page = 1, string? search = null)
    {
        var result = await api.GetWorkspacesPageAsync(UserId, page, search, ct);
        return Json(result.Data ?? new PaginatedResult<WorkspaceResponse>([], page, 20, 0));
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
    public async Task<IActionResult> WorkspaceData(int id, CancellationToken ct, string? search = null, int page = 1)
    {
        var data = await WorkspaceDataAsync(search, page, id, ct);
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
    public async Task<IActionResult> DeleteWorkspace(int id, CancellationToken ct, int page = 1, string? search = null)
    {
        await api.DeleteWorkspaceAsync(id, ct);

        var result = await api.GetWorkspacesPageAsync(UserId, page, search, ct);
        if ((result.Data?.Items.Count ?? 0) == 0 && page > 1)
        {
            result = await api.GetWorkspacesPageAsync(UserId, page - 1, search, ct);
            page--;
        }

        return Json(result.Data ?? new PaginatedResult<WorkspaceResponse>([], page, 20, 0));
    }

    [HttpPost("teacher/workspaces/{id:int}/students")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWorkspaceStudents(int id, AddWorkspaceStudentsViewModel model, CancellationToken ct)
    {
        await api.AddStudentsAsync(id, new AddStudentsRequest(model.StudentIds), ct);
        var data = await WorkspaceDataAsync(null, 1, id, ct);
        return data is null ? NotFound() : Json(data);
    }

    [HttpDelete("teacher/workspaces/{id:int}/students/{studentId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveWorkspaceStudent(int id, int studentId, CancellationToken ct)
    {
        await api.RemoveStudentAsync(id, studentId, ct);
        var data = await WorkspaceDataAsync(null, 1, id, ct);
        return data is null ? NotFound() : Json(data);
    }

    [HttpGet("teacher/library")]
    public IActionResult Library() => View("Library/Index");

    [HttpGet("teacher/library/tree")]
    public async Task<IActionResult> LibraryTree(CancellationToken ct)
    {
        var result = await api.GetLibraryTreeAsync(UserId, ct);
        return Json(result.Data ?? []);
    }

    [HttpGet("teacher/library/contents")]
    public async Task<IActionResult> LibraryContents(CancellationToken ct)
    {
        var result = await api.GetContentsAsync(UserId, ct);
        return Json(result.Data ?? []);
    }

    [HttpPost("teacher/library/folder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLibraryFolder([FromBody] CreateFolderRequest model, CancellationToken ct)
    {
        var result = await api.CreateFolderAsync(new CreateFolderRequest(null, UserId, model.Name, model.ParentId), ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpPost("teacher/library/content")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLibraryContent([FromBody] CreateContentRequest model, CancellationToken ct)
    {
        var result = await api.CreateContentAsync(new CreateContentRequest(UserId, model.Title, model.Description, model.Type, model.Url, model.ParentId), ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpPut("teacher/library/content/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLibraryContent(int id, [FromBody] UpdateContentRequest model, CancellationToken ct)
    {
        var result = await api.UpdateContentAsync(id, model, ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpPut("teacher/library/content/{id:int}/publish")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetContentPublished(int id, bool published, CancellationToken ct)
    {
        var result = await api.SetContentPublishedAsync(id, published, ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpDelete("teacher/library/content/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLibraryContent(int id, CancellationToken ct)
    {
        var result = await api.DeleteContentAsync(id, ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpPut("teacher/nodes/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNode(int id, [FromBody] UpdateNodeRequest model, CancellationToken ct)
    {
        var result = await api.UpdateNodeAsync(id, model, ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpDelete("teacher/nodes/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNode(int id, CancellationToken ct)
    {
        var result = await api.DeleteNodeAsync(id, ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpGet("teacher/workspaces/{id:int}/tree")]
    public async Task<IActionResult> WorkspaceTree(int id, CancellationToken ct)
    {
        var result = await api.GetWorkspaceTreeAsync(id, ct);
        return Json(result.Data ?? []);
    }

    [HttpPost("teacher/workspaces/{id:int}/folder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWorkspaceFolder(int id, [FromBody] CreateFolderRequest model, CancellationToken ct)
    {
        var result = await api.CreateFolderAsync(new CreateFolderRequest(id, null, model.Name, model.ParentId), ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpPost("teacher/workspaces/{id:int}/content")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWorkspaceContent(int id, [FromBody] AddContentRequest model, CancellationToken ct)
    {
        var result = await api.AddContentToWorkspaceAsync(new AddContentRequest(id, model.ContentId, model.ParentId), ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    [HttpPost("teacher/workspaces/{id:int}/copy-folder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CopyFolderToWorkspace(int id, [FromBody] CopyFolderRequest model, CancellationToken ct)
    {
        var result = await api.CopyFolderToWorkspaceAsync(new CopyFolderRequest(id, model.SourceNodeId, model.ParentId), ct);
        return result.Success ? Json(new { success = true }) : ApiError(result);
    }

    private IActionResult ApiError<T>(ApiResult<T> result)
    {
        var message = result.Message
            ?? result.Errors?.FirstOrDefault()
            ?? L.T("common.error");
        Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        return Json(new { success = false, message });
    }

    [HttpGet("teacher/attendance")]
    public async Task<IActionResult> Attendance(int? workspaceId, string? month, CancellationToken ct)
    {
        var (year, monthNumber) = ParseMonth(month);
        var workspacesResult = await api.GetWorkspacesPageAsync(UserId, 1, null, ct);
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

    public async Task<IActionResult> Settings(CancellationToken ct)
    {
        ViewData["TitleKey"] = "nav.settings";
        var config = await api.GetSiteConfigAsync(ct);
        var model = config.Data is null
            ? new SiteSettingsViewModel()
            : new SiteSettingsViewModel
            {
                SiteName = config.Data.SiteName,
                NavMode = config.Data.NavMode,
                NotificationProvider = config.Data.NotificationProvider,
                PrimaryColor = config.Data.PrimaryColor,
                SecondaryColor = config.Data.SecondaryColor,
                SuccessColor = config.Data.SuccessColor,
                WarningColor = config.Data.WarningColor,
                ErrorColor = config.Data.ErrorColor,
                InfoColor = config.Data.InfoColor,
            };
        return View(model);
    }

    [HttpPost("teacher/settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(SiteSettingsViewModel model, CancellationToken ct)
    {
        var result = await api.UpdateSiteConfigAsync(new UpdateSiteConfigRequest(
            model.SiteName.Trim(),
            model.PrimaryColor, model.SecondaryColor, model.SuccessColor,
            model.WarningColor, model.ErrorColor, model.InfoColor,
            model.NavMode, model.NotificationProvider), ct);

        if (!result.Success)
        {
            foreach (var error in result.Errors ?? [])
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }
        }

        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            return View("Settings", model);
        }

        TempData["Success"] = L.T("settings.saved");
        return IsAjax ? Json(new { success = true }) : RedirectToAction(nameof(Settings));
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

    private async Task<WorkspaceDataResponse?> WorkspaceDataAsync(string? search, int page, int id, CancellationToken ct)
    {
        var workspace = await api.GetWorkspaceAsync(id, ct);
        if (!workspace.Success || workspace.Data is null)
        {
            return null;
        }

        var inWorkspace = workspace.Data.Students.Select(s => s.Id).ToHashSet();
        var inWorkspaceMatches = workspace.Data.Students.Count(s => MatchesStudentSearch(s, search));
        var totalMatching = 0;

        var available = new List<StudentResponse>();
        var wantedStart = (page - 1) * 20;
        var wantedEnd = page * 20;
        var apiPage = 1;

        while (available.Count < wantedEnd)
        {
            var result = await api.GetStudentsPageAsync(apiPage, search, ct);
            if (result.Data is null)
            {
                break;
            }

            totalMatching = result.Data.TotalCount;
            available.AddRange(result.Data.Items.Where(s => !inWorkspace.Contains(s.UserId)));

            var lastPage = (int)Math.Ceiling(totalMatching / 20.0);
            if (apiPage >= lastPage)
            {
                break;
            }

            apiPage++;
        }

        var totalAvailable = Math.Max(0, totalMatching - inWorkspaceMatches);
        var totalPages = totalAvailable == 0 ? 1 : (int)Math.Ceiling(totalAvailable / 20.0);
        var pageItems = available.Skip(wantedStart).Take(20).ToList();

        return new WorkspaceDataResponse(workspace.Data, pageItems, page, totalPages, totalAvailable);
    }

    private static bool MatchesStudentSearch(WorkspaceStudentResponse s, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return true;
        var q = search.Trim().ToLowerInvariant();
        return (s.FirstName + " " + s.LastName).ToLowerInvariant().Contains(q)
            || s.Email.ToLowerInvariant().Contains(q);
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailPattern();
}
