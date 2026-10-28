using Kela.Core.Common.Caching;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Setting.Abstractions;
using Kela.Core.Modules.Setting.Requests;
using Kela.Core.Modules.Tenant.Abstractions;
using Kela.Core.Modules.Tenant.Requests;
using Kela.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

[Authorize(Policy = AuthPolicies.Admin)]
[Route("[controller]")]
public sealed class SettingController(ISettingService service, ITenantService tenants, IAppCache cache) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/setting/index.js" };
        return View(new SettingPageModel
        {
            Setting = await service.GetAsync(ct),
            Tenant = await tenants.GetCurrentAsync(ct)
        });
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromForm] SettingUpsertRequest request, CancellationToken ct)
    {
        // Tətbiq versiyaları bütün tenant-lara aiddir — yalnız superadmin dəyişə bilər.
        if (!User.IsInRole(AuthRoles.SuperAdmin))
        {
            var current = await service.GetAsync(ct);
            request.AndroidMinVersion = current.AndroidMinVersion;
            request.IosMinVersion = current.IosMinVersion;
        }

        return Json(await service.UpdateAsync(request, ct));
    }

    /// <summary>Kurs (tenant) adının yenilənməsi — yalnız cari tenant.</summary>
    [HttpPut("profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateTenantProfileRequest request, CancellationToken ct)
        => Json(await tenants.UpdateCurrentAsync(request, ct));

    [HttpPost("clear-cache")]
    [ValidateAntiForgeryToken]
    public IActionResult ClearCache()
    {
        cache.Clear();
        return Json(new { ok = true });
    }
}
