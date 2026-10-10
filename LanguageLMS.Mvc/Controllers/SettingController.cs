using LanguageLMS.Core.Modules.Setting.Abstractions;
using LanguageLMS.Core.Modules.Setting.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Mvc.Controllers;

[Route("[controller]")]
public sealed class SettingController(ISettingService service) : Controller
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
}
