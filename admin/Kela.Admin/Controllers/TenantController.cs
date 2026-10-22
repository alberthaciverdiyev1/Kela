using System.Security.Claims;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Tenant.Abstractions;
using Kela.Core.Modules.Subscription.Abstractions;
using Kela.Core.Modules.Subscription.Requests;
using Kela.Core.Modules.Tenant.Requests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

[Authorize(Policy = AuthPolicies.SuperAdmin)]
[Route("[controller]")]
public sealed class TenantController(ITenantService service, ISubscriptionService subscriptions) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/tenant/list.js", "~/js/pages/tenant/create.js", "~/js/pages/tenant/plan.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await service.ListAsync(ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken ct)
        => Json(await service.CreateAsync(request, ct));

    [HttpGet("plan-list")]
    public async Task<IActionResult> PlanList(CancellationToken ct)
        => Json(await subscriptions.ListPlansAsync(onlyActive: true, ct));

    /// <summary>Superadmin tenant-a plan təyin edir (və ya planı dəyişir).</summary>
    [HttpPut("{id:long}/plan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPlan(long id, [FromForm] AssignPlanRequest request, CancellationToken ct)
    {
        await subscriptions.AssignPlanAsync(id, request, ct);
        return Json(new { ok = true });
    }

    /// <summary>Ödənişdən sonra əlavə paket (məs. +10 tələbə yeri).</summary>
    [HttpPost("{id:long}/addons")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddon(long id, [FromForm] AddAddonRequest request, CancellationToken ct)
        => Json(await subscriptions.AddAddonAsync(id, request, ct));

    [HttpGet("pending-count")]
    public async Task<IActionResult> PendingCount(CancellationToken ct)
        => Json(new { count = await service.CountPendingAsync(ct) });

    /// <summary>Superadmin müraciəti rədd edir — pending tenant silinir.</summary>
    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

    /// <summary>Superadmin müraciəti təsdiqləyir/deaktiv edir.</summary>
    [HttpPut("{id:long}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(long id, [FromBody] SetTenantStatusRequest request, CancellationToken ct)
        => Json(await service.SetActiveAsync(id, request.IsActive, ct));

    /// <summary>Superadmin seçilmiş tenant-ın kontekstinə keçir (cookie-yə tenant_id/tenant_name claim).</summary>
    [HttpPost("switch/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Switch(long id, CancellationToken ct)
    {
        var tenant = await service.GetByIdAsync(id, ct);
        if (tenant is null) return NotFound();

        await ReSignInAsync(tenant.Id, tenant.Name);
        return Json(new { ok = true });
    }

    /// <summary>Superadmin tenant kontekstindən çıxır (yenidən qlobal).</summary>
    [HttpPost("switch-off")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchOff()
    {
        await ReSignInAsync(null, null);
        return RedirectToAction("Index", "Dashboard");
    }

    private async Task ReSignInAsync(long? tenantId, string? tenantName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""),
            new(ClaimTypes.Name, User.FindFirst(ClaimTypes.Name)?.Value ?? ""),
            new(ClaimTypes.Email, User.FindFirst(ClaimTypes.Email)?.Value ?? ""),
            new(ClaimTypes.Role, User.FindFirst(ClaimTypes.Role)?.Value ?? AuthRoles.SuperAdmin)
        };

        if (tenantId is > 0)
        {
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
            claims.Add(new Claim("tenant_name", tenantName ?? ""));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
