using Kela.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kela.Web.Filters;

public sealed class SessionExpiredFilter : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not ApiSessionExpiredException)
        {
            return;
        }

        context.ExceptionHandled = true;
        var http = context.HttpContext;

        if (http.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Result = new JsonResult(new { success = false, message = "Oturum süresi doldu. Lütfen tekrar giriş yapın." })
            {
                StatusCode = StatusCodes.Status401Unauthorized,
            };
            return;
        }

        await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        http.Response.Cookies.Delete(AppConstants.ApiAuthCookie, new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = true,
        });
        context.Result = new RedirectResult("/auth/login");
    }
}
