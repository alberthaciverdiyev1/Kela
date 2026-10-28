using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Subscription.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>
/// Superadmin üçün platforma gəliri — bütün tenant-lardan gələn abunəlik və əlavə paket ödənişləri.
/// Tenant kassasından (tələbə ödənişləri) ayrı səhifədir.
/// </summary>
[Authorize(Policy = AuthPolicies.SuperAdmin)]
[Route("[controller]")]
public sealed class PlatformFinanceController(IPlatformFinanceService finance) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!User.IsPlatformOwner())
        {
            return RedirectToAction("Index", "Tenant");
        }

        ViewData["Scripts"] = new[] { "~/js/pages/platform-finance/index.js" };
        ViewData["Title"] = "Platform gəliri";
        return View(await finance.GetAsync(12, ct));
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await finance.GetAsync(12, ct));
}
