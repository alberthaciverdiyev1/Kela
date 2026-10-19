using Kela.Core.Modules.User.Entities;
using Kela.Core.Common.Localization;
using Kela.Core.Common.Sorting;
using Kela.Core.Common.Responses;
using Kela.Core.Data;
using Kela.Core.Modules.User.Requests;
using Kela.Core.Modules.User.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Kela.Core.Modules.User.Services;

using User = Kela.Core.Modules.User.Entities.User;

public sealed class UserService(AppDbContext db, IStringLocalizer<SharedResource> L) : IUserService
{
    public async Task<PagedResponse<UserResponse>> ListAsync(UserListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage < 1 ? 20 : Math.Min(request.PerPage, 500);

        // Superadmin qlobal hesabdır — istifadəçi siyahısında göstərilmir.
        var query = db.Users.AsNoTracking().Where(x => x.Type != UserTypes.SuperAdmin);

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters(new[] { "SoftDelete" }).Where(x => x.DeletedAt != null);
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(x => x.Type == request.Type);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Email, $"%{search}%") ||
                (x.Name != null && EF.Functions.ILike(x.Name, $"%{search}%")) ||
                (x.Surname != null && EF.Functions.ILike(x.Surname, $"%{search}%")));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .ApplyListSort(request)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<UserResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<UserResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var user = await db.Users.IgnoreQueryFilters(new[] { "SoftDelete" }).AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"User not found: {id}");

        return ToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(UserUpsertRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        await EnsureEmailAvailableAsync(email, null, ct);

        var user = new User
        {
            Name = request.Name,
            Surname = request.Surname,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            Type = request.Type
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(long id, UserUpsertRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"User not found: {id}");

        var email = request.Email.Trim().ToLowerInvariant();
        await EnsureEmailAvailableAsync(email, id, ct);

        user.Name = request.Name;
        user.Surname = request.Surname;
        user.Email = email;
        user.PhoneNumber = request.PhoneNumber;
        user.Type = request.Type;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await db.SaveChangesAsync(ct);

        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateProfileAsync(long id, UserProfileRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"User not found: {id}");

        var email = request.Email.Trim().ToLowerInvariant();
        await EnsureEmailAvailableAsync(email, id, ct);

        user.Name = request.Name;
        user.Surname = request.Surname;
        user.Email = email;
        user.PhoneNumber = request.PhoneNumber;

        await db.SaveChangesAsync(ct);
        return ToResponse(user);
    }

    public async Task ChangePasswordAsync(long id, UserChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"User not found: {id}");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new ArgumentException(L["Api_CurrentPasswordIncorrect"]);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"User not found: {id}");

        user.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureEmailAvailableAsync(string email, long? excludeId, CancellationToken ct)
    {
        var exists = await db.Users.IgnoreQueryFilters(new[] { "SoftDelete" })
            .AnyAsync(x => x.Email == email && (excludeId == null || x.Id != excludeId), ct);

        if (exists)
        {
            throw new ArgumentException("A user with this email already exists");
        }
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Surname, user.Email, user.PhoneNumber, user.Type);
}
