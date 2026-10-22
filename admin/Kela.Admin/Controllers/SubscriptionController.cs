using Kela.Admin.Infrastructure;
using Kela.Core.Modules.Subscription.Abstractions;
using Kela.Core.Modules.Subscription.Requests;
using Kela.Core.Modules.Subscription.Responses;
using Kela.Core.Modules.Tenant.Abstractions;
using Kela.Core.Modules.User.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

/// <summary>Tenant-ın öz abunəliyi: cari plan, istifadə və əlavə paketlər.</summary>
[Route("[controller]")]
public sealed class SubscriptionController(
    ISubscriptionService service,
    ITenantService tenants) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(long? tenantId, CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/subscription/index.js" };
        ViewData["IsAdmin"] = User.IsAdmin();

        var selected = await ResolveTenantAsync(tenantId, ct);

        if (User.IsInRole(UserTypes.SuperAdmin))
        {
            ViewData["Tenants"] = await tenants.ListAsync(ct);
        }

        ViewData["SelectedTenantId"] = selected;

        // Superadmin tenant seçməyibsə plan yoxdur — səhifə seçim tələb edir.
        SubscriptionResponse? model = selected is long id ? await service.GetAsync(id, ct) : null;

        return View(model);
    }

    [HttpGet("data")]
    public async Task<IActionResult> Data(long? tenantId, CancellationToken ct)
    {
        var selected = await ResolveTenantAsync(tenantId, ct);
        if (selected is not long id) return BadRequest(new { message = "Tenant seçilməyib" });

        return Json(await service.GetAsync(id, ct));
    }

    /// <summary>Əlavə paket — hazırda yalnız admin/superadmin əlavə edir (ödəniş əl ilə).</summary>
    [HttpPost("addons")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddon([FromForm] AddAddonRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin()) return Forbid();

        var tenantId = request.TenantId
            ?? await ResolveTenantAsync(null, ct);

        if (tenantId is not > 0) return BadRequest(new { message = "Tenant seçilməyib" });

        return Json(await service.AddAddonAsync(tenantId.Value, request, ct));
    }

    [HttpGet("addons")]
    public async Task<IActionResult> Addons(long? tenantId, CancellationToken ct)
    {
        var selected = await ResolveTenantAsync(tenantId, ct);
        if (selected is not long id) return BadRequest(new { message = "Tenant seçilməyib" });

        return Json(await service.ListAddonsAsync(id, ct));
    }

    /// <summary>Superadmin üçün seçilmiş tenant: query → keçmiş tenant claim → ilk tenant.</summary>
    private async Task<long?> ResolveTenantAsync(long? tenantId, CancellationToken ct)
    {
        if (!User.IsInRole(UserTypes.SuperAdmin))
        {
            var own = User.CurrentTenantId();
            return own > 0 ? own : null;
        }

        if (tenantId is > 0) return tenantId;

        var switched = User.CurrentTenantId();
        if (switched > 0) return switched;

        var all = await tenants.ListAsync(ct);
        return all.FirstOrDefault()?.Id;
    }
}
