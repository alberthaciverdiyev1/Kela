using Edutor.Core.Common.MultiTenancy;
using Edutor.Core.Data;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Auth.Services;
using Edutor.Core.Modules.User.Entities;
using Edutor.Core.Modules.User.Requests;
using Edutor.Core.Modules.User.Responses;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.User.Services;

using User = Edutor.Core.Modules.User.Entities.User;

public sealed class UserAuthService(AppDbContext db, ITokenService tokens, ITenantProvider tenantProvider) : IUserAuthService
{
    public async Task<UserAuthResponse> LoginAsync(UserLoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var requestedTenant = request.TenantId ?? tenantProvider.TenantId;

        // Tenant açıq verilməyibsə (admin panel girişi) hesab email ilə bütün
        // tenant-larda axtarılır; qeydiyyat email-in qlobal unikal olmasını təmin edir.
        var user = requestedTenant is long scopeTenant
            ? await db.Users.IgnoreQueryFilters()
                .Where(x => x.Email == email && x.DeletedAt == null && (x.TenantId == 0 || x.TenantId == scopeTenant))
                .OrderByDescending(x => x.TenantId == scopeTenant)
                .ThenByDescending(x => x.TenantId == 0)
                .FirstOrDefaultAsync(ct)
            : await db.Users.IgnoreQueryFilters()
                .Where(x => x.Email == email && x.DeletedAt == null)
                .OrderByDescending(x => x.TenantId == 0)
                .ThenBy(x => x.Id)
                .FirstOrDefaultAsync(ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Email or password is incorrect");
        }

        // Superadmin qlobal hesabdır — tenant tələb olunmur. Digərləri yalnız öz tenant-ında
        // və tenant aktiv olduqda giriş edə bilər.
        if (user.Type != UserTypes.SuperAdmin)
        {
            if (requestedTenant is long tenantId && user.TenantId != tenantId)
            {
                throw new UnauthorizedAccessException("Email or password is incorrect");
            }

            if (user.TenantId <= 0 || !await db.Tenants.AnyAsync(t => t.Id == user.TenantId && t.IsActive, ct))
            {
                throw new UnauthorizedAccessException("Tenant is not active");
            }
        }

        var token = tokens.CreateToken(user.Id, user.Email, user.Name ?? user.Email, user.Type, user.TenantId);
        return new UserAuthResponse(token, ToResponse(user));
    }

    public async Task<UserResponse> GetProfileAsync(long userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new KeyNotFoundException($"User not found: {userId}");

        return ToResponse(user);
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Surname, user.Email, user.PhoneNumber, user.Type, user.TenantId);
}
