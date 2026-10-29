using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Subscription.Requests;
using Edutor.Core.Modules.Subscription.Responses;
using Edutor.Core.Modules.Tenant.Abstractions;
using Edutor.Core.Modules.User.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Tenant-ın limitləri: cari plan, istifadə və əlavə paketlər.</summary>
[Route("[controller]")]
public sealed class SubscriptionController(
    ISubscriptionService service,
    ITenantService tenants) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery(Name = "tenant_id")] long? tenantId, CancellationToken ct)
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

        return View(model);
    }

    [HttpGet("data")]
    public async Task<IActionResult> Data([FromQuery(Name = "tenant_id")] long? tenantId, CancellationToken ct)
    {
        var selected = await ResolveTenantAsync(tenantId, ct);
        if (selected is not long id) return BadRequest(new { message = "Tenant seçilməyib" });

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
                new { message = "Limit paketini yalnız platform administratoru əlavə edə bilər." });
        }

        var tenantId = request.TenantId
            ?? await ResolveTenantAsync(null, ct);

        if (tenantId is not > 0) return BadRequest(new { message = "Tenant seçilməyib" });

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
    public async Task<IActionResult> AddonRequests([FromQuery(Name = "tenant_id")] long? tenantId, string? status, CancellationToken ct)
    {
        var selected = await ResolveTenantAsync(tenantId, ct);
        if (selected is not long id) return BadRequest(new { message = "Tenant seçilməyib" });

        return Json(await service.ListAddonRequestsAsync(id, status, ct));
    }

    private Task<long?> ResolveTenantAsync(long? tenantId, CancellationToken ct)
        => TenantPageContext.ResolveAsync(User, tenantId, tenants, ct);
}
