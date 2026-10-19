using Kela.Core.Modules.Dashboard.Abstractions;
using Kela.Admin.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class DashboardController(IDashboardService dashboard) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/dashboard/index.js" };

        var ownerId = User.IsAdmin() ? null : User.CurrentUserId();
        return View(await dashboard.GetAsync(ownerId, ct));
    }
}
