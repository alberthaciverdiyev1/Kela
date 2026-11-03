using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Subscription.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;

namespace Edutor.Admin.Controllers;

/// <summary>
/// Superadmin üçün platforma gəliri — bütün tenant-lardan gələn abunəlik və əlavə paket ödənişləri.
/// Tenant cashboxsından (tələbə ödənişləri) ayrı səhifədir.
/// </summary>
[Authorize(Policy = AuthPolicies.SuperAdmin)]
[Route("[controller]")]
public sealed class PlatformFinanceController(
    IPlatformFinanceService finance,
    ISubscriptionService subscriptions,
    IStringLocalizer<SharedResource> L) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!User.IsPlatformOwner())
        {
            return RedirectToAction("Index", "Tenant");
        }

        ViewData["Scripts"] = new[]
        {
            "~/js/pages/platform-finance/index.js",
            "~/js/pages/platform-finance/subscriptions.js"
        };
        ViewData["Title"] = L["Revenue_Title"];
        return View(await finance.GetAsync(12, ct));
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await finance.GetAsync(12, ct));

    /// <summary>Abunəliyi yeniləyir, planı dəyişir və ya müddəti uzadır — yalnız superadmin.</summary>
    [HttpPut("{tenantId:long}/subscription")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenewSubscription(
        long tenantId, [FromForm] RenewSubscriptionRequest request, CancellationToken ct)
    {
        if (!User.IsPlatformOwner())
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = L["PlatformFinance_PlatformOnly"].Value });
        }

        try
        {
            return Json(await subscriptions.RenewAsync(tenantId, request, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
