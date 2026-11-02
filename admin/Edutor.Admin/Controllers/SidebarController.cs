using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Sidebar.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Sidebar sayğacları, "yeni" işarələri və platforma etiketləri (JSON).</summary>
[Route("[controller]")]
public sealed class SidebarController(ISidebarService sidebar) : Controller
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
        => Json(await sidebar.GetSummaryAsync(User.CurrentUserId() ?? 0, ct));

    [HttpPost("seen/{key}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Seen(string key, CancellationToken ct)
    {
        if (User.CurrentUserId() is long userId) await sidebar.MarkSeenAsync(userId, key, ct);
        return Json(new { ok = true });
    }
}
