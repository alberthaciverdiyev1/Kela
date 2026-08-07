using System.Text.RegularExpressions;
using Kela.Web.Infrastructure;
using Kela.Web.Localization;
using Kela.Web.Models.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Web.Controllers;

[Authorize(Roles = AppConstants.RoleTeacher)]
public sealed partial class TeacherController(IApiClient api, Localizer L) : Controller
{
    private const int PageSize = 10;

    public IActionResult Dashboard() => View();

    public async Task<IActionResult> Students(CancellationToken ct)
    {
        var result = await api.GetStudentsPageAsync(1, PageSize, ct);
        return View(new StudentsIndexViewModel(
            result.Data?.Items ?? [], result.Data?.Page ?? 1, PageSize, result.Data?.TotalCount ?? 0, null));
    }

    [HttpGet("teacher/students/table")]
    public async Task<IActionResult> StudentsTable(CancellationToken ct, int page = 1, string? search = null)
    {
        var result = await api.GetStudentsPageAsync(page, PageSize, ct);
        return PartialView("_StudentsTable", new StudentsIndexViewModel(
            result.Data?.Items ?? [], result.Data?.Page ?? 1, PageSize, result.Data?.TotalCount ?? 0, search));
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
            return PartialView("_StudentsCreateFields", model);
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

            return PartialView("_StudentsCreateFields", model);
        }

        var list = await api.GetStudentsPageAsync(1, PageSize, ct);
        var listModel = new StudentsIndexViewModel(
            list.Data?.Items ?? [], list.Data?.Page ?? 1, PageSize, list.Data?.TotalCount ?? 0, null);

        Response.Headers["HX-Trigger"] = "kela:createDone";

        return PartialView("_StudentsCreateSuccess", new CreateStudentSuccessViewModel(listModel, created.Data!));
    }

    [HttpDelete("teacher/students/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudent(int id, CancellationToken ct, int page = 1, string? search = null)
    {
        await api.DeleteStudentAsync(id, ct);

        var result = await api.GetStudentsPageAsync(page, PageSize, ct);
        if ((result.Data?.Items.Count ?? 0) == 0 && page > 1)
        {
            result = await api.GetStudentsPageAsync(page - 1, PageSize, ct);
            page--;
        }

        return PartialView("_StudentsTable", new StudentsIndexViewModel(
            result.Data?.Items ?? [], result.Data?.Page ?? 1, PageSize, result.Data?.TotalCount ?? 0, search));
    }

    public IActionResult Classes()
    {
        ViewData["TitleKey"] = "nav.classes";
        return View("~/Views/Shared/ComingSoon.cshtml");
    }

    public IActionResult Settings()
    {
        ViewData["TitleKey"] = "nav.settings";
        return View("~/Views/Shared/ComingSoon.cshtml");
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailPattern();
}
