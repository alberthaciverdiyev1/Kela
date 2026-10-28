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

    /// <summary>Əlavə paket — yalnız admin/superadmin (ödəniş əl ilə).</summary>
    [HttpPost("addons")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddon([FromForm] AddAddonRequest request, CancellationToken ct)
    {
        // Cookie auth-да [Authorize] 302 ilə login-ə yönləndirir və JS xətanı
        // görmür; ona görə aydın 403 JSON qaytarırıq.
        if (!User.IsInRole(UserTypes.Admin) && !User.IsInRole(UserTypes.SuperAdmin))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Limit artırmaq üçün administratorla əlaqə saxlayın." });
        }

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

    /// <summary>Limit artırma müraciəti — müəllim və kurs (admin) müraciət edə bilər.</summary>
    [HttpPost("addon-requests")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAddonRequest([FromForm] AddonRequestBody request, CancellationToken ct)
    {
        var tenantId = User.CurrentTenantId();
        if (tenantId <= 0) return BadRequest(new { message = "Tenant seçilməyib" });

        try
        {
            var created = await service.CreateAddonRequestAsync(
                tenantId, request, User.CurrentUserId(), User.Identity?.Name, ct);

            return Json(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("addon-requests")]
    public async Task<IActionResult> AddonRequests(string? status, CancellationToken ct)
    {
        var selected = await ResolveTenantAsync(null, ct);
        if (selected is not long id) return BadRequest(new { message = "Tenant seçilməyib" });

        return Json(await service.ListAddonRequestsAsync(id, status, ct));
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
