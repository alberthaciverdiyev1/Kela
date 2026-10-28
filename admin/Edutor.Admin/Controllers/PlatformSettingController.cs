using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Setting.Abstractions;
using Edutor.Core.Modules.Setting.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Superadmin üçün platforma (ümumi) əlaqə parametrləri.</summary>
[Authorize(Policy = AuthPolicies.SuperAdmin)]
[Route("[controller]")]
public sealed class PlatformSettingController(ISettingService service) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/platform-setting/index.js" };
        return View(await service.GetPlatformAsync(ct));
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromForm] SettingUpsertRequest request, CancellationToken ct)
        => Json(await service.UpdatePlatformAsync(request, ct));
}
