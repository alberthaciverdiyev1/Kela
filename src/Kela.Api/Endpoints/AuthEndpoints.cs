using System.Security.Claims;
using Kela.Application.Users.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Kela.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", Login);
        group.MapPost("/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        }).RequireAuthorization();

        return app;
    }

    /// <summary>
    /// Kimlik doğrulama: başarılıysa claim'leri içeren imzalı+şifreli cookie yazar (cookie-only yaklaşım).
    /// </summary>
    private static async Task<IResult> Login(
        LoginRequest request,
        IAuthService auth,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var result = await auth.LoginAsync(request.Email, request.Password, ct);

        if (result is null)
        {
            return Results.Problem(
                title: "E-posta veya şifre hatalı.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new(ClaimTypes.Name, $"{result.FirstName} {result.LastName}"),
            new(ClaimTypes.Role, result.Role.ToString()),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Results.Ok(result);
    }

    public sealed record LoginRequest(string Email, string Password);
}
