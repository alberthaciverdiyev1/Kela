using Kela.Application.Repositories;
using Kela.Domain.Enums;

namespace Kela.Application.Users.Auth;

internal sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher) : IAuthService
{
    public async Task<LoginResult?> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(normalizedEmail, cancellationToken);

        // Kullanıcı yoksa da aynı sonucu dön → user enumeration koruması
        if (user is null
            || user.Status != UserStatus.Active
            || !passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return new LoginResult(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Role);
    }
}
