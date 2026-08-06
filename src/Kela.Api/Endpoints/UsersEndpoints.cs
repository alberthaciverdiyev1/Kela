using Kela.Application.Users;
using Kela.Application.Users.Requests;

namespace Kela.Api.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        group.MapGet("", async (int page, int pageSize, IUserService users, CancellationToken ct) =>
            Results.Ok(await users.GetPageAsync(page, pageSize, ct)));

        group.MapGet("/{id:int}", async (int id, IUserService users, CancellationToken ct) =>
        {
            var user = await users.GetByIdAsync(id, ct);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        group.MapPost("", async (CreateUserRequest request, IUserService users, CancellationToken ct) =>
        {
            var id = await users.CreateAsync(request, ct);
            return Results.Created($"/api/users/{id}", new { id });
        });

        group.MapPut("/{id:int}", async (int id, UpdateUserRequest request, IUserService users, CancellationToken ct) =>
        {
            await users.UpdateAsync(id, request, ct);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (int id, IUserService users, CancellationToken ct) =>
        {
            await users.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        return app;
    }
}
