using System.Security.Claims;
using System.Text.RegularExpressions;
using Kela.Web.Helpers;
using Kela.Web.Localization;
using Kela.Web.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Web.Controllers;

[AllowAnonymous]
public sealed partial class AuthController(IApiClient api, Localizer l) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(AppConstants.HomeRouteFor(User.FindFirstValue(ClaimTypes.Role) ?? ""));
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!IsLoginValid(model))
        {
            return LoginError(model);
        }

        var result = await api.LoginAsync(model.Email.Trim(), model.Password, ct);

        if (!result.Success || result.Data is null || string.IsNullOrEmpty(result.SetCookie))
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Giriş başarısız oldu.");
            return LoginError(model);
        }

        Response.Cookies.Append(AppConstants.ApiAuthCookie, result.SetCookie, AuthCookieOptions(TimeSpan.FromHours(8)));

        var data = result.Data;
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, data.UserId.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, $"{data.FirstName} {data.LastName}".Trim()));
        identity.AddClaim(new Claim(ClaimTypes.Role, data.Role));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        var redirectUrl = (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            ? model.ReturnUrl
            : AppConstants.HomeRouteFor(data.Role);

        return IsAjax ? Json(new { redirect = redirectUrl }) : Redirect(redirectUrl);
    }

    private IActionResult LoginError(LoginViewModel model)
    {
        if (IsAjax)
        {
            Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            return PartialView("_LoginFields", model);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        if (!IsRegisterValid(model))
        {
            return RegisterError(model);
        }

        var result = await api.RegisterAsync(new RegisterRequest(
            model.FirstName.Trim(), model.LastName?.Trim() ?? "", model.Email.Trim(), model.Password), ct);

        if (!result.Success)
        {
            if (result.Errors is not null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return RegisterError(model);
        }

        TempData["Success"] = l.T("auth.registerSuccess");
        return IsAjax ? Json(new { redirect = "/auth/login" }) : RedirectToAction(nameof(Login), "Auth");
    }

    private IActionResult RegisterError(RegisterViewModel model)
    {
        if (IsAjax)
        {
            Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            return PartialView("_RegisterFields", model);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await api.LogoutAsync(ct);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete(AppConstants.ApiAuthCookie, AuthCookieOptions(null));
        return IsAjax ? Json(new { redirect = "/auth/login" }) : RedirectToAction(nameof(Login), "Auth");
    }

    private bool IsAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    private bool IsLoginValid(LoginViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), l.T("auth.reqEmail"));
        }
        else if (!EmailPattern().IsMatch(model.Email.Trim()))
        {
            ModelState.AddModelError(nameof(model.Email), l.T("auth.invalidEmail"));
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), l.T("auth.reqPassword"));
        }

        return ModelState.IsValid;
    }

    private bool IsRegisterValid(RegisterViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
        {
            ModelState.AddModelError(nameof(model.FirstName), l.T("auth.reqFirstName"));
        }

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), l.T("auth.reqEmail"));
        }
        else if (!EmailPattern().IsMatch(model.Email.Trim()))
        {
            ModelState.AddModelError(nameof(model.Email), l.T("auth.invalidEmail"));
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), l.T("auth.reqPassword"));
        }
        else if (model.Password.Length < 6)
        {
            ModelState.AddModelError(nameof(model.Password), l.T("auth.passwordMin"));
        }

        if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), l.T("auth.reqConfirmPassword"));
        }
        else if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), l.T("auth.passwordMismatch"));
        }

        return ModelState.IsValid;
    }

    private static CookieOptions AuthCookieOptions(TimeSpan? expires)
    {
        var options = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = true,
        };

        if (expires is not null)
        {
            options.Expires = DateTimeOffset.UtcNow.Add(expires.Value);
        }

        return options;
    }

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailPattern();
}
