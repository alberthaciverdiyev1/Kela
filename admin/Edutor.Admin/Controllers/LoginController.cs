using FluentValidation;
using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Tenant.Abstractions;
using Edutor.Core.Modules.User.Abstractions;
using Edutor.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Edutor.Admin.Controllers;

[AllowAnonymous]
[Route("[controller]")]
public sealed class LoginController(
    IUserAuthService service,
    ITenantService tenants,
    IValidator<UserLoginRequest> validator) : Controller
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
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return await LoginViewAsync(validation.Errors[0].ErrorMessage, ct);
        }

        try
        {
            var result = await service.LoginAsync(request, ct);

            await AdminSignIn.SignInAsync(HttpContext, result.User, request.TenantId);

            return RedirectToAction("Index", "Dashboard");
        }
        catch (UnauthorizedAccessException)
        {
            return await LoginViewAsync("Email, şifrə və ya tenant yanlışdır.", ct);
        }
    }

    private async Task<IActionResult> LoginViewAsync(string error, CancellationToken ct)
    {
        ViewData["Error"] = error;
        ViewData["Tenants"] = await tenants.ListPublicAsync(ct);
        ViewData["Scripts"] = new[] { "~/js/pages/login/index.js" };
        return View("Index");
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index");
    }
}
