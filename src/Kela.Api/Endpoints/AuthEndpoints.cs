using Kela.Api.Contracts;
using Kela.Application.Features.Users.Auth;
using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Features.Users.Auth.Responses;
using Kela.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Kela.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", Login);
        group.MapPost("/logout", async (SignInManager<User> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return ApiResponse.NoContent();
        }).RequireAuthorization();

        return app;
    }

    /// <summary>
    /// Giriş: IAuthService → SignInManager ile doğrular, lockout uygular ve
    /// security stamp içeren Identity cookie'sini yazar (cookie-only yaklaşım).
    /// </summary>
    private static async Task<IResult> Login(
        LoginRequest request,
        IAuthService auth,
        CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);

        if (result is null)
        {
            return ApiResponse.Unauthorized("E-posta veya şifre hatalı.");
        }

        return ApiResponse<LoginResponse>.Success(result);
    }
}
