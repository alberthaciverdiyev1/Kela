using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Subscription.Requests;
using Edutor.Core.Modules.Tenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>
/// Limit artırma müraciətləri — müəllim/kurs müraciət edir, admin təsdiqləyir.
/// Aylıq paketlər buradan dayandırılır.
/// </summary>
[Authorize(Policy = AuthPolicies.Admin)]
[Route("[controller]")]
public sealed class AddonRequestController(
    ISubscriptionService subscriptions,
    ITenantService tenants) : AdminControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/addon-request/index.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(string? status, CancellationToken ct)
    {
        var tenantId = ScopeTenantId();
        var rows = await subscriptions.ListAddonRequestsAsync(tenantId, status, ct);

        // Superadmin bütün tenantları görür — adları burada qoşuruq.
        if (tenantId is null && rows.Count > 0)
        {
            var names = (await tenants.ListAsync(ct)).ToDictionary(x => x.Id, x => x.Name);
            rows = [.. rows.Select(x => x with { TenantName = names.GetValueOrDefault(x.TenantId) })];
        }

        return Json(rows);
    }

    [HttpGet("addons")]
    public async Task<IActionResult> Addons(long tenantId, CancellationToken ct)
        => Json(await subscriptions.ListAddonsAsync(tenantId, ct));

    [HttpPost("{id:long}/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long id, AddonReviewRequest request, CancellationToken ct)
    {
        try
        {
            var addon = await subscriptions.ApproveAddonRequestAsync(
                id, User.CurrentUserId(), User.Identity?.Name, request, ct);

            return Json(addon);
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:long}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(long id, AddonReviewRequest request, CancellationToken ct)
    {
        try
        {
            await subscriptions.RejectAddonRequestAsync(
                id, User.CurrentUserId(), User.Identity?.Name, request, ct);

            return Json(new { ok = true });
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("addons/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAddon(long id, CancellationToken ct)
    {
        await subscriptions.CancelAddonAsync(id, ct);
        return Json(new { ok = true });
    }

    /// <summary>Superadmin tenant seçmədən bütün müraciətləri görür.</summary>
    private long? ScopeTenantId()
    {
        var current = User.CurrentTenantId();
        if (current > 0) return current;

        return User.IsInRole(AuthRoles.SuperAdmin) ? null : current;
    }
}
