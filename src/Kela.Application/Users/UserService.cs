using Kela.Application.Pagination;
using Kela.Application.Repositories;
using Kela.Application.Users.Requests;
using Kela.Application.Users.Responses;
using Kela.Application.Validation;
using Kela.Domain.Entities;
using Kela.Domain.Enums;

namespace Kela.Application.Users;

internal sealed class UserService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IValidator<CreateUserRequest> createValidator,
    IValidator<UpdateUserRequest> updateValidator) : IUserService
{
    public async Task<PaginatedResult<UserResponse>> GetPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await users.GetPageAsync(page, pageSize, cancellationToken);
        return new PaginatedResult<UserResponse>(
            result.Items.Select(u => u.ToResponse()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        return user?.ToResponse();
    }

    public async Task<int> CreateAsync(
        CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        createValidator.Validate(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await users.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException($"'{normalizedEmail}' email adresi zaten kayıtlı.");
        }

        var user = new User(request.FirstName.Trim(), request.LastName.Trim(), normalizedEmail)
        {
            CreatedAt = DateTime.UtcNow,
        };

        user.SetPasswordHash(passwordHasher.Hash(request.Password));
        // Rol ↔ profil tutarlılığını domain garantiler: yalnızca role uyan tek profil kurulur.
        user.AssignProfile(request.Role);

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
            user.SetPasswordHash(passwordHasher.Hash(request.Password));
        }

        if (request.Status is not null)
        {
            user.SetStatus(request.Status.Value);
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
