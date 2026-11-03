using FluentValidation;
using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.User.Abstractions;
using Edutor.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;

namespace Edutor.Admin.Controllers;

[AllowAnonymous]
[Route("[controller]")]
public sealed class LoginController(
    IUserAuthService service,
    IValidator<UserLoginRequest> validator,
    IStringLocalizer<SharedResource> L) : Controller
{
    [HttpGet]
    public IActionResult Index(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Dashboard");

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
            return LoginView(validation.Errors[0].ErrorMessage);
        }

        try
        {
            var result = await service.LoginAsync(request, ct);

            await AdminSignIn.SignInAsync(HttpContext, result.User, result.User.TenantId);

            return RedirectToAction("Index", "Dashboard");
        }
        catch (UnauthorizedAccessException)
        {
            return LoginView(L["Login_InvalidCredentials"]);
        }
    }

    private IActionResult LoginView(string error)
    {
        ViewData["Error"] = error;
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
