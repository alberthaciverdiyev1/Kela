using Kela.Api.Contracts;
using Kela.Application.Features.Users;
using Kela.Application.Features.Users.Requests;
using Kela.Application.Features.Users.Responses;
using Kela.Application.Pagination;

namespace Kela.Api.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        group.MapGet("", async (int page, int pageSize, IUserService users, CancellationToken ct) =>
            ApiResponse<PaginatedResult<UserResponse>>.Success(
                await users.GetPageAsync(page, pageSize, ct)));

        group.MapGet("/{id:int}", async (int id, IUserService users, CancellationToken ct) =>
        {
            var user = await users.GetByIdAsync(id, ct);
            return user is null
                ? ApiResponse.NotFound("Kayıt bulunamadı.")
                : ApiResponse<UserResponse>.Success(user);
        });

        group.MapPost("", async (CreateUserRequest request, IUserService users, CancellationToken ct) =>
        {
            var id = await users.CreateAsync(request, ct);
            return ApiResponse<object>.Created($"/api/users/{id}", new { id });
        });

        group.MapPut("/{id:int}", async (int id, UpdateUserRequest request, IUserService users, CancellationToken ct) =>
        {
            await users.UpdateAsync(id, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, IUserService users, CancellationToken ct) =>
        {
            await users.DeleteAsync(id, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
