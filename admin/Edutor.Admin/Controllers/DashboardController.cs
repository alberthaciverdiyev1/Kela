using Edutor.Core.Modules.Dashboard.Abstractions;
using Edutor.Core.Modules.Payments.Abstractions;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Tenant.Abstractions;
using Edutor.Admin.Infrastructure;
using Edutor.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class DashboardController(
    IDashboardService dashboard,
    IKassaService kassa,
    IPlatformFinanceService platformFinance,
    ITenantService tenants) : Controller
{
    private const string IndividualTeacherAccount = "teacher";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userName = User.Identity?.Name ?? string.Empty;

        // Superadmin tenant seçməyibsə platformanın ümumi mənzərəsini görür.
        if (User.IsPlatformOwner())
        {
            ViewData["Scripts"] = new[]
            {
                "~/js/pages/dashboard/index.js",
                "~/js/pages/platform-finance/index.js"
            };
            return View("Platform", new PlatformDashboardModel
            {
                Finance = await platformFinance.GetAsync(12, ct),
                UserName = userName
            });
        }

        ViewData["Scripts"] = new[] { "~/js/pages/dashboard/index.js" };

        var ownerId = User.IsAdmin() ? null : User.CurrentUserId();

        // Kassa sidebar-dakı eyni qayda ilə görünür: admin və ya fərdi müəllim kursu.
        var canSeeFinance = User.IsAdmin()
            || await tenants.GetCurrentAccountTypeAsync() == IndividualTeacherAccount;

        return View(new DashboardPageModel
        {
            Stats = await dashboard.GetAsync(ownerId, ct),
            Finance = canSeeFinance ? await kassa.GetOverviewAsync(ct) : null,
            UserName = userName
        });
    }
}
