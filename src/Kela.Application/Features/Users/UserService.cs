using FluentValidation;
using Kela.Application.Features.Users.Requests;
using Kela.Application.Features.Users.Responses;
using Kela.Application.Pagination;
using Kela.Domain.Entities;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Users;

internal sealed class UserService(
    IUserRepository users,
    UserManager<User> userManager,
    IValidator<UpdateUserRequest> updateValidator) : IUserService
{
    public async Task<PaginatedResult<UserResponse>> GetPageAsync(
        int page, CancellationToken cancellationToken = default)
    {
        var result = await users.GetPageAsync(page, cancellationToken);

        var items = new List<UserResponse>(result.Items.Count);
        foreach (var user in result.Items)
        {
            items.Add(user.ToResponse(await user.ResolveRoleAsync(userManager)));
        }

        return new PaginatedResult<UserResponse>(items, result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        return user is null ? null : user.ToResponse(await user.ResolveRoleAsync(userManager));
    }

    public async Task UpdateAsync(
        int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await users.GetByIdAsync(id, cancellationToken)
                   ?? throw new KeyNotFoundException($"Id = {id} olan kullanıcı bulunamadı.");

        user.SetName(request.FirstName, request.LastName);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            // .NET Identity'nin PasswordHasher'ı (PBKDF2) kullanılır.
            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, request.Password);
        }

        if (request.Status is not null)
        {
            user.SetStatus(request.Status.Value);
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(id, cancellationToken)
                   ?? throw new KeyNotFoundException($"Id = {id} olan kullanıcı bulunamadı.");

        // Soft delete
        user.DeletedAt = DateTime.UtcNow;
        user.SetStatus(UserStatus.Inactive);

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
