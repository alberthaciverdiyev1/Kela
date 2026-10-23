using System.Security.Claims;
using Kela.Core.Modules.Tenant.Abstractions;
using Kela.Core.Modules.User.Abstractions;
using Kela.Core.Modules.User.Entities;
using Kela.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kela.Admin.Controllers;

[AllowAnonymous]
[Route("[controller]")]
public sealed class LoginController(IUserAuthService service, ITenantService tenants) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Dashboard");

        ViewData["Tenants"] = await tenants.ListPublicAsync(ct);
        ViewData["Scripts"] = new[] { "~/js/pages/login/index.js" };
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Index(UserLoginRequest request, CancellationToken ct)
    {
        try
        {
            var result = await service.LoginAsync(request, ct);
            var user = result.User;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, $"{user.Name} {user.Surname}".Trim()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Type)
            };

            // Tenant claim — sonrakı sorğularda məlumat yalnız bu tenant-la məhdudlaşır.
            if (user.Type != UserTypes.SuperAdmin && request.TenantId is > 0)
            {
                claims.Add(new Claim("tenant_id", request.TenantId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Dashboard");
        }
        catch (UnauthorizedAccessException)
        {
            ViewData["Error"] = "Email, şifrə və ya tenant yanlışdır.";
            ViewData["Tenants"] = await tenants.ListPublicAsync(ct);
            ViewData["Scripts"] = new[] { "~/js/pages/login/index.js" };
            return View();
        }
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index");
    }
}
