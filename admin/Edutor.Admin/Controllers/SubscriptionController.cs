using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Subscription.Requests;
using Edutor.Core.Modules.Subscription.Responses;
using Edutor.Core.Modules.Tenant.Abstractions;
using Edutor.Core.Modules.User.Entities;
using Microsoft.AspNetCore.Mvc;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;

namespace Edutor.Admin.Controllers;

/// <summary>Tenant-ın limitləri: cari plan, istifadə və əlavə paketlər.</summary>
[Route("[controller]")]
public sealed class SubscriptionController(
    ISubscriptionService service,
    ITenantService tenants,
    IStringLocalizer<SharedResource> L) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery(Name = "tenant_id")] long? tenantId, CancellationToken ct)
    {
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/subscription/index.js",
            "~/js/helpers/plan-modal.js",
            "~/js/helpers/limit-overrides-modal.js"
        };

        var selected = await ResolveTenantAsync(tenantId, ct);

        if (User.IsInRole(UserTypes.SuperAdmin))
        {
            ViewData["Tenants"] = await tenants.ListAsync(ct);
        }

        ViewData["SelectedTenantId"] = selected;
        ViewData["TenantActive"] = selected is long tenantKey
            ? (await tenants.GetByIdAsync(tenantKey, ct))?.IsActive
            : null;

        // Superadmin tenant seçməyibsə plan yoxdur — səhifə seçim tələb edir.
        SubscriptionResponse? model = selected is long id ? await service.GetAsync(id, ct) : null;

        return View(model);
    }

    /// <summary>Müştəriyə xüsusi limitlər — yalnız platform administratoru.</summary>
    [HttpGet("{tenantId:long}/limits")]
    public async Task<IActionResult> Limits(long tenantId, CancellationToken ct)
    {
        if (!User.IsInRole(UserTypes.SuperAdmin)) return Forbidden();

        return Json(await service.GetLimitOverridesAsync(tenantId, ct));
    }

    [HttpPut("{tenantId:long}/limits")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetLimits(long tenantId, [FromForm] LimitOverridesRequest request, CancellationToken ct)
    {
        if (!User.IsInRole(UserTypes.SuperAdmin)) return Forbidden();

        try
        {
            await service.SetLimitOverridesAsync(tenantId, request, ct);
            return Json(new { ok = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Aylıq istifadə tarixçəsi — yalnız platform administratoru.</summary>
    [HttpGet("{tenantId:long}/usage-history")]
    public async Task<IActionResult> UsageHistory(long tenantId, CancellationToken ct)
    {
        if (!User.IsInRole(UserTypes.SuperAdmin)) return Forbidden();

        return Json(await service.ListUsageHistoryAsync(tenantId, 12, ct));
    }

    private IActionResult Forbidden()
        => StatusCode(StatusCodes.Status403Forbidden,
            new { message = L["Subscription_PlatformOnly"].Value });

    [HttpGet("data")]
    public async Task<IActionResult> Data([FromQuery(Name = "tenant_id")] long? tenantId, CancellationToken ct)
    {
        var selected = await ResolveTenantAsync(tenantId, ct);
        if (selected is not long id) return BadRequest(new { message = L["Tenant_NotSelected"].Value });

        return Json(await service.GetAsync(id, ct));
    }

    /// <summary>
    /// Əlavə pakət — yalnız superadmin. Paketin birdəfəlik/aylıq forması və
    /// ödəniş vaxtı superadmin qərarıdır; tenant admin yalnız müraciət göndərir.
    /// </summary>
    [HttpPost("addons")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddon([FromForm] AddAddonRequest request, CancellationToken ct)
    {
        // Cookie auth-да [Authorize] 302 ilə login-ə yönləndirir və JS xətanı
        // görmür; ona görə aydın 403 JSON qaytarırıq.
        if (!User.IsInRole(UserTypes.SuperAdmin))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = L["Subscription_PlatformOnlyAddon"].Value });
        }

        var tenantId = request.TenantId
            ?? await ResolveTenantAsync(null, ct);

        if (tenantId is not > 0) return BadRequest(new { message = L["Tenant_NotSelected"].Value });

        try
        {
            return Json(await service.AddAddonAsync(tenantId.Value, request, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("addons")]
    public async Task<IActionResult> Addons([FromQuery(Name = "tenant_id")] long? tenantId, CancellationToken ct)
    {
        var selected = await ResolveTenantAsync(tenantId, ct);
        if (selected is not long id) return BadRequest(new { message = L["Tenant_NotSelected"].Value });

        return Json(await service.ListAddonsAsync(id, ct));
    }

    /// <summary>Limit artırma müraciəti — müəllim və kurs (admin) müraciət edə bilər.</summary>
    [HttpPost("addon-requests")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAddonRequest([FromForm] AddonRequestBody request, CancellationToken ct)
    {
        var tenantId = User.CurrentTenantId();
        if (tenantId <= 0) return BadRequest(new { message = L["Tenant_NotSelected"].Value });

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
    public async Task<IActionResult> AddonRequests([FromQuery(Name = "tenant_id")] long? tenantId, string? status, CancellationToken ct)
    {
        var selected = await ResolveTenantAsync(tenantId, ct);
        if (selected is not long id) return BadRequest(new { message = L["Tenant_NotSelected"].Value });

        // Admin/müəllim yalnız öz müraciətlərini görür; superadmin hamısını.
        var ownUserId = User.IsInRole(UserTypes.SuperAdmin) ? null : User.CurrentUserId();

        return Json(await service.ListAddonRequestsAsync(id, status, ownUserId, ct));
    }

    private Task<long?> ResolveTenantAsync(long? tenantId, CancellationToken ct)
        => TenantPageContext.ResolveAsync(User, tenantId, tenants, ct);
}
