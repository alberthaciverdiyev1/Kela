using System.Security.Claims;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Tenant.Abstractions;
using Kela.Core.Modules.Tenant.Requests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

[Authorize(Policy = AuthPolicies.SuperAdmin)]
[Route("[controller]")]
public sealed class TenantController(ITenantService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/tenant/list.js", "~/js/pages/tenant/create.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await service.ListAsync(ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken ct)
        => Json(await service.CreateAsync(request, ct));

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
