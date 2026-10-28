using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Tenant.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>
/// Qeydiyyatdan sonra plan seçimi. Tenant plan seçməyibsə panelin bütün
/// səhifələrindən bura yönləndirilir (bax: PlanSelectionFilter).
/// </summary>
[Route("plan-choose")]
public sealed class PlanChooseController(
    ISubscriptionService subscriptions,
    ITenantService tenants) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var tenantId = User.CurrentTenantId();
        if (tenantId <= 0) return RedirectToAction("Index", "Dashboard");

        var tenant = await tenants.GetByIdAsync(tenantId, ct);
        if (tenant is null) return RedirectToAction("Index", "Dashboard");

        // Plan artıq seçilibsə bu səhifəyə ehtiyac yoxdur.
        if (tenant.PlanId is not null) return RedirectToAction("Index", "Dashboard");

        var plans = await subscriptions.ListPlansAsync(onlyActive: true, ct);
        if (plans.Count == 0) return RedirectToAction("Index", "Dashboard");

        ViewData["Title"] = "Plan seçimi";
        ViewData["TenantName"] = tenant.Name;
        return View(plans);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Select([FromForm] long planId, CancellationToken ct)
    {
        var tenantId = User.CurrentTenantId();
        if (tenantId <= 0) return RedirectToAction("Index", "Dashboard");

        try
        {
            await subscriptions.SelectPlanAsync(tenantId, planId, ct);
        }
        catch (ArgumentException ex)
        {
            ViewData["Error"] = ex.Message;
            return await Index(ct);
        }

        return RedirectToAction("Index", "Dashboard");
    }
}
