using Kela.Core.Modules.PrivacyPolicy.Abstractions;
using Kela.Core.Modules.Tenant.Abstractions;
using Kela.Core.Modules.Tenant.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

/// <summary>Public kurs müraciəti — tenant superadmin təsdiqinə qədər aktiv olmur.</summary>
[AllowAnonymous]
[Route("register")]
public sealed class RegisterController(ITenantService tenants, IPrivacyPolicyService policies) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Dashboard");
        return View();
    }

    /// <summary>Müraciət edən üçün istifadə şərtləri / məxfilik siyasəti.</summary>
    [HttpGet("terms")]
    public async Task<IActionResult> Terms(CancellationToken ct)
    {
        string? content = null;

        foreach (var type in new[] { "terms", "privacy" })
        {
            var policy = await policies.GetPublicAsync(type, ct);
            if (!string.IsNullOrWhiteSpace(policy?.Content))
            {
                content = policy.Content;
                break;
            }
        }

        ViewData["Content"] = content;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegisterTenantRequest request, CancellationToken ct)
    {
        try
        {
            await tenants.RegisterAsync(request, ct);
            ViewData["Success"] = true;
            return View(new RegisterTenantRequest());
        }
        catch (ArgumentException ex)
        {
            ViewData["Error"] = ex.Message;
            return View(request);
        }
    }
}
