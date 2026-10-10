using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Auth.Services;
using LanguageLMS.Core.Modules.User.Requests;
using LanguageLMS.Core.Modules.User.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.User.Services;

using User = LanguageLMS.Core.Modules.User.Entities.User;

public sealed class UserAuthService(AppDbContext db, ITokenService tokens) : IUserAuthService
{
    public async Task<UserAuthResponse> LoginAsync(UserLoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Email or password is incorrect");
        }

        var token = tokens.CreateToken(user.Id, user.Email, user.Name ?? user.Email, user.Type);
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
