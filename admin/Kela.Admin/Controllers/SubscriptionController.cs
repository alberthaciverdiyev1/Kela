using Kela.Admin.Infrastructure;
using Kela.Core.Modules.Subscription.Abstractions;
using Kela.Core.Modules.Subscription.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

/// <summary>Tenant-ın öz abunəliyi: cari plan, istifadə və əlavə paketlər.</summary>
[Route("[controller]")]
public sealed class SubscriptionController(ISubscriptionService service) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/subscription/index.js" };
        ViewData["IsAdmin"] = User.IsAdmin();

        return View(await service.GetCurrentAsync(ct));
    }

    [HttpGet("data")]
    public async Task<IActionResult> Data(CancellationToken ct)
        => Json(await service.GetCurrentAsync(ct));

    /// <summary>Əlavə paket — hazırda yalnız admin/superadmin əlavə edir (ödəniş əl ilə).</summary>
    [HttpPost("addons")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddon([FromForm] AddAddonRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin()) return Forbid();

        var tenantId = request.TenantId ?? User.CurrentTenantId();
        if (tenantId <= 0) return BadRequest(new { message = "Tenant seçilməyib" });

        return Json(await service.AddAddonAsync(tenantId, request, ct));
    }

    [HttpGet("addons")]
    public async Task<IActionResult> Addons(CancellationToken ct)
        => Json(await service.ListAddonsAsync(User.CurrentTenantId(), ct));
}
