using Kela.Application.Pagination;
using Kela.Application.Repositories;
using Kela.Application.Users.Dtos;
using Kela.Domain.Entities;
using Kela.Domain.Enums;

namespace Kela.Application.Users;

internal sealed class UserService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher) : IUserService
{
    public async Task<PaginatedResult<UserDto>> GetPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await users.GetPageAsync(page, pageSize, cancellationToken);
        return new PaginatedResult<UserDto>(
            result.Items.Select(u => u.ToDto()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        return user?.ToDto();
    }

    public async Task<int> CreateAsync(
        string firstName, string lastName, string email, string password, Role role,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (await users.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException($"'{normalizedEmail}' email adresi zaten kayıtlı.");
        }

        var user = new User(firstName.Trim(), lastName.Trim(), normalizedEmail)
        {
            CreatedAt = DateTime.UtcNow,
        };

        user.SetPasswordHash(passwordHasher.Hash(password));
        // Rol ↔ profil tutarlılığını domain garantiler: yalnızca role uyan tek profil kurulur.
        user.AssignProfile(role);

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }

    public async Task UpdateAsync(
        int id, string firstName, string lastName, string? password, UserStatus? status,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan kullanıcı bulunamadı.");

        user.SetName(firstName, lastName);

        if (!string.IsNullOrWhiteSpace(password))
        {
            user.SetPasswordHash(passwordHasher.Hash(password));
        }

        if (status is not null)
        {
            user.SetStatus(status.Value);
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan kullanıcı bulunamadı.");

        // Soft delete
        user.DeletedAt = DateTime.UtcNow;
        user.SetStatus(UserStatus.Inactive);

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
