using Kela.Application.Features.Users.Requests;
using Kela.Application.Features.Users.Responses;
using Kela.Application.Pagination;
using Kela.Application.Validation;
using Kela.Domain.Entities;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Users;

internal sealed class UserService(
    IUserRepository users,
    UserManager<User> userManager,
    RoleManager<IdentityRole<int>> roleManager,
    IValidator<CreateUserRequest> createValidator,
    IValidator<UpdateUserRequest> updateValidator) : IUserService
{
    public async Task<PaginatedResult<UserResponse>> GetPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await users.GetPageAsync(page, pageSize, cancellationToken);

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

    public async Task<int> CreateAsync(
        CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        createValidator.Validate(request);

        var email = request.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();

        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            throw new InvalidOperationException($"'{normalizedEmail}' email adresi zaten kayıtlı.");
        }

        var user = new User(request.FirstName.Trim(), request.LastName.Trim(), email)
        {
            CreatedAt = DateTime.UtcNow,
        };

        // Rol ↔ profil tutarlılığını domain garantiler: yalnızca role uyan tek profil kurulur.
        user.AssignProfile(request.Role);

        // Identity: parolayı hash'ler (PBKDF2), kullanıcıyı + profilini kaydeder.
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        // Identity rol üyeliği (AspNetUserRoles).
        var roleName = request.Role.ToString();
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
        await userManager.AddToRoleAsync(user, roleName);

        return user.Id;
    }

    public async Task UpdateAsync(
        int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        updateValidator.Validate(request);

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
