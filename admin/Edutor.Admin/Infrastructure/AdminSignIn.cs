using System.Security.Claims;
using Edutor.Core.Modules.User.Entities;
using Edutor.Core.Modules.User.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Edutor.Admin.Infrastructure;

public static class AdminSignIn
{
    /// <summary>MVC panelin cookie-sini yaradır (login və qeydiyyatdan sonrakı avtomatik giriş).</summary>
    public static async Task SignInAsync(HttpContext http, UserResponse user, long? tenantId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.Name} {user.Surname}".Trim()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Type)
        };

        // Tenant claim — sonrakı sorğularda məlumat yalnız bu tenant-la məhdudlaşır.
        if (user.Type != UserTypes.SuperAdmin && tenantId is > 0)
        {
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
