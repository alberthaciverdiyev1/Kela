using Kela.Core.Modules.Dashboard.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

[Route("[controller]")]
public sealed class DashboardController(IDashboardService dashboard) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/dashboard/index.js" };

        return View(await dashboard.GetAsync(ct));
    }
}
