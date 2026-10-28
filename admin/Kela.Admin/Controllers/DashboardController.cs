using Kela.Core.Modules.Dashboard.Abstractions;
using Kela.Core.Modules.Payments.Abstractions;
using Kela.Core.Modules.Tenant.Abstractions;
using Kela.Admin.Infrastructure;
using Kela.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class DashboardController(
    IDashboardService dashboard,
    IKassaService kassa,
    ITenantService tenants) : Controller
{
    private const string IndividualTeacherAccount = "teacher";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/dashboard/index.js" };

        var ownerId = User.IsAdmin() ? null : User.CurrentUserId();

        // Kassa sidebar-dakı eyni qayda ilə görünür: admin və ya fərdi müəllim kursu.
        var canSeeFinance = User.IsAdmin()
            || await tenants.GetCurrentAccountTypeAsync() == IndividualTeacherAccount;

        return View(new DashboardPageModel
        {
            Stats = await dashboard.GetAsync(ownerId, ct),
            Finance = canSeeFinance ? await kassa.GetOverviewAsync(ct) : null,
            UserName = User.Identity?.Name ?? string.Empty
        });
    }
}
