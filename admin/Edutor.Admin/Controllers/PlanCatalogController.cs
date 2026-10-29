using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Setting.Abstractions;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Subscription.Responses;
using Edutor.Core.Modules.Tenant.Abstractions;
using Edutor.Core.Modules.User.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>
/// Tenant üçün mövcud abunəlik planları. Limitlər səhifəsindən ayrıdır — plan
/// seçimi WhatsApp müraciəti kimi göndərilir, aktivləşdirmə əl ilə aparılır.
/// </summary>
[Authorize(Policy = AuthPolicies.Admin)]
[Route("plan-catalog")]
public sealed class PlanCatalogController(
    ISubscriptionService service,
    ITenantService tenants,
    ISettingService settings) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery(Name = "tenant_id")] long? tenantId, CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/plan-catalog/index.js" };

        var selected = await ResolveTenantAsync(tenantId, ct);

        if (User.IsInRole(UserTypes.SuperAdmin))
        {
            ViewData["Tenants"] = await tenants.ListAsync(ct);
        }

        ViewData["SelectedTenantId"] = selected;

        SubscriptionResponse? model = selected is long id ? await service.GetAsync(id, ct) : null;

        if (selected is long tenantIdValue)
        {
            var tenant = await tenants.GetByIdAsync(tenantIdValue, ct);
            var platform = await settings.GetPlatformAsync(ct);

            ViewData["TenantName"] = tenant?.Name ?? string.Empty;
            ViewData["Plans"] = await service.ListPlansAsync(onlyActive: true, ct);

            // Plan seçimi hazırda WhatsApp ilə göndərilir — nömrə superadmin-in
            // platforma ayarlarından götürülür, yoxdursa sabit nömrə.
            ViewData["WhatsAppNumber"] = TenantPageContext.NormalizeWhatsApp(platform.WhatsappNumber);
        }

        return View(model);
    }

    private Task<long?> ResolveTenantAsync(long? tenantId, CancellationToken ct)
        => TenantPageContext.ResolveAsync(User, tenantId, tenants, ct);
}
