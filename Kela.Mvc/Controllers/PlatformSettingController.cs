using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Setting.Abstractions;
using Kela.Core.Modules.Setting.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

/// <summary>Superadmin üçün platforma (ümumi) əlaqə parametrləri.</summary>
[Authorize(Policy = AuthPolicies.SuperAdmin)]
[Route("[controller]")]
public sealed class PlatformSettingController(ISettingService service) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Platforma parametrləri";
        ViewData["Scripts"] = new[] { "~/js/pages/platform-setting/index.js" };
        return View(await service.GetPlatformAsync(ct));
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromForm] SettingUpsertRequest request, CancellationToken ct)
        => Json(await service.UpdatePlatformAsync(request, ct));
}
