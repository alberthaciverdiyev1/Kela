using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.PrivacyPolicy.Abstractions;
using Edutor.Core.Modules.Tenant.Abstractions;
using Edutor.Core.Modules.Tenant.Requests;
using Edutor.Core.Modules.User.Abstractions;
using Edutor.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Public kurs müraciəti — qeydiyyatdan sonra avtomatik giriş və plan seçimi.</summary>
[AllowAnonymous]
[Route("register")]
public sealed class RegisterController(
    ITenantService tenants,
    IPrivacyPolicyService policies,
    IUserAuthService auth) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/register/index.js" };
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
    [EnableRateLimiting("public")]
    public async Task<IActionResult> Index(RegisterTenantRequest request, CancellationToken ct)
    {
        // Honeypot: botlar bu gizli sahəni doldurur — onlara uğurlu cavab göstəririk.
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            ViewData["Success"] = true;
            ViewData["Scripts"] = new[] { "~/js/pages/register/index.js" };
            return View(new RegisterTenantRequest());
        }

        try
        {
            var tenant = await tenants.RegisterAsync(request, ct);

            // Qeydiyyatdan dərhal sonra daxil edirik — istifadəçi plan seçimini görsün.
            var login = await auth.LoginAsync(new UserLoginRequest
            {
                Email = request.Email,
                Password = request.Password,
                TenantId = tenant.Id
            }, ct);

            await AdminSignIn.SignInAsync(HttpContext, login.User, tenant.Id);

            return RedirectToAction("Index", "PlanChoose");
        }
        catch (UnauthorizedAccessException)
        {
            // Avtomatik giriş alınmasa hesab yenə yaranıb — adi girişə yönləndiririk.
            ViewData["Success"] = true;
            ViewData["Scripts"] = new[] { "~/js/pages/register/index.js" };
            return View(new RegisterTenantRequest());
        }
        catch (ArgumentException ex)
        {
            ViewData["Error"] = ex.Message;
            ViewData["Scripts"] = new[] { "~/js/pages/register/index.js" };
            return View(request);
        }
    }
}
