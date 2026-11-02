using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Sidebar;
using Edutor.Core.Modules.Sidebar.Abstractions;
using Edutor.Core.Modules.Sidebar.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Superadmin sidebar bölmələri üçün platforma etiketi təyin edir; bütün tenant-lar görür.</summary>
[Authorize(Policy = AuthPolicies.SuperAdmin)]
[Route("[controller]")]
public sealed class SidebarLabelController(ISidebarService sidebar) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Labels"] = await sidebar.ListLabelsAsync(ct);
        ViewData["Scripts"] = new[] { "~/js/pages/sidebar-label/index.js" };
        return View(SidebarKeys.All);
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await sidebar.ListLabelsAsync(ct));

    [HttpPut("{key}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string key, [FromBody] SidebarLabelRequest request, CancellationToken ct)
    {
        try
        {
            return Json(await sidebar.SetLabelAsync(key, request, ct));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
