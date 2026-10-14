using Kela.Core.Common.Caching;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Setting.Abstractions;
using Kela.Core.Modules.Setting.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

[Authorize(Policy = AuthPolicies.Admin)]
[Route("[controller]")]
public sealed class SettingController(ISettingService service, IAppCache cache) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Parametrlər";
        ViewData["Scripts"] = new[] { "~/js/pages/setting/index.js" };
        return View(await service.GetAsync(ct));
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromForm] SettingUpsertRequest request, CancellationToken ct)
        => Json(await service.UpdateAsync(request, ct));

    [HttpPost("clear-cache")]
    [ValidateAntiForgeryToken]
    public IActionResult ClearCache()
    {
        cache.Clear();
        return Json(new { ok = true });
    }
}
