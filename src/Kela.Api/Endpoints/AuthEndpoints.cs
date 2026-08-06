using Kela.Api.Contracts;
using Kela.Application.Features.Users.Auth;
using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Features.Users.Auth.Responses;

namespace Kela.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/logout", Logout).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IAuthService auth,
        CancellationToken ct)
    {
        var result = await auth.RegisterAsync(request, ct);
        return ApiResponse<RegisterResponse>.Created($"/api/users/{result.UserId}", result);
    }

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

    private static async Task<IResult> Logout(IAuthService auth, CancellationToken ct)
    {
        await auth.LogoutAsync(ct);
        return ApiResponse.NoContent();
    }
}
