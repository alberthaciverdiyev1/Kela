using Kela.Core.Common.MultiTenancy;
using Kela.Core.Data;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Auth.Services;
using Kela.Core.Modules.User.Entities;
using Kela.Core.Modules.User.Requests;
using Kela.Core.Modules.User.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kela.Core.Modules.User.Services;

using User = Kela.Core.Modules.User.Entities.User;

public sealed class UserAuthService(AppDbContext db, ITokenService tokens, ITenantProvider tenantProvider, IConfiguration config) : IUserAuthService
{
    private bool StrictTenant => config.GetValue<bool>("Tenancy:Strict");

    public async Task<UserAuthResponse> LoginAsync(UserLoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Email or password is incorrect");
        }

        // Superadmin qlobal hesabdır — tenant tələb olunmur. Digərləri yalnız öz tenant-ında.
        if (user.Type != UserTypes.SuperAdmin)
        {
            var tenantId = request.TenantId ?? tenantProvider.TenantId;

            if (tenantId is null)
            {
                if (StrictTenant) throw new UnauthorizedAccessException("Tenant is required");
            }
            else
            {
                if (!await db.Tenants.AnyAsync(t => t.Id == tenantId.Value && t.IsActive, ct))
                {
                    throw new UnauthorizedAccessException("Tenant is not active");
                }

                if (user.TenantId != tenantId.Value)
                {
                    throw new UnauthorizedAccessException("Email or password is incorrect");
                }
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
        new(user.Id, user.Name, user.Surname, user.Email, user.PhoneNumber, user.Type);
}
