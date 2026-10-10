using LanguageLMS.Core.Modules.Dashboard.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Mvc.Controllers;

public sealed class DashboardController(IDashboardService dashboard) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Dashboard";
        return View(await dashboard.GetAsync(ct));
    }
}
