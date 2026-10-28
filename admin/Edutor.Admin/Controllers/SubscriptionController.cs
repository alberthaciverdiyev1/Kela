using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Subscription.Requests;
using Edutor.Core.Modules.Subscription.Responses;
using Edutor.Core.Modules.Setting.Abstractions;
using Edutor.Core.Modules.Tenant.Abstractions;
using Edutor.Core.Modules.User.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Tenant-ın öz abunəliyi: cari plan, istifadə və əlavə paketlər.</summary>
[Route("[controller]")]
public sealed class SubscriptionController(
    ISubscriptionService service,
    ITenantService tenants,
    ISettingService settings) : AdminControllerBase
{
    private const string DefaultWhatsApp = "994709990569";

    [HttpGet]
    public async Task<IActionResult> Index(long? tenantId, CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/subscription/index.js" };

        var selected = await ResolveTenantAsync(tenantId, ct);

        if (User.IsInRole(UserTypes.SuperAdmin))
        {
            ViewData["Tenants"] = await tenants.ListAsync(ct);
        }

        ViewData["SelectedTenantId"] = selected;

        // Superadmin tenant seçməyibsə plan yoxdur — səhifə seçim tələb edir.
        SubscriptionResponse? model = selected is long id ? await service.GetAsync(id, ct) : null;

        if (selected is long tenantIdValue)
        {
            var tenant = await tenants.GetByIdAsync(tenantIdValue, ct);
            var platform = await settings.GetPlatformAsync(ct);

            ViewData["TenantName"] = tenant?.Name ?? string.Empty;
            ViewData["Plans"] = await service.ListPlansAsync(onlyActive: true, ct);

            // Plan seçimi hazırda WhatsApp ilə göndərilir — nömrə superadmin-in
            // platforma ayarlarından götürülür, yoxdursa sabit nömrə.
            ViewData["WhatsAppNumber"] = NormalizeWhatsApp(platform.WhatsappNumber);
        }

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

    private static string NormalizeWhatsApp(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());

        if (digits.StartsWith("00")) digits = digits[2..];
        if (digits.Length == 0) return DefaultWhatsApp;

        // Yerli format (0XX XXX XX XX) → beynəlxalq (994XX...).
        if (digits.Length == 10 && digits.StartsWith('0')) digits = "994" + digits[1..];

        return digits;
    }

    /// <summary>Superadmin üçün seçilmiş tenant: query → keçmiş tenant claim → ilk tenant.</summary>
    private async Task<long?> ResolveTenantAsync(long? tenantId, CancellationToken ct)
    {
        if (!User.IsInRole(UserTypes.SuperAdmin))
        {
            var own = User.CurrentTenantId();
            return own > 0 ? own : null;
        }

        if (tenantId is > 0)
        {
            return await tenants.GetByIdAsync(tenantId.Value, ct) is not null ? tenantId : null;
        }

        var switched = User.CurrentTenantId();
        if (switched > 0) return switched;

        var all = await tenants.ListAsync(ct);
        return all.FirstOrDefault()?.Id;
    }
}
