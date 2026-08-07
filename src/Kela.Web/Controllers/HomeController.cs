using System.Security.Claims;
using Kela.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Web.Controllers;

public sealed class HomeController : Controller
{
    [Authorize]
    public IActionResult Index()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        return Redirect(AppConstants.HomeRouteFor(role));
    }

    [AllowAnonymous]
    public IActionResult Blocked() => View();

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Lang(string lang, string? returnUrl)
    {
        if (AppConstants.Langs.Contains(lang))
        {
            Response.Cookies.Append(AppConstants.LangCookie, lang, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
            });
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return Redirect("/");
    }
}
