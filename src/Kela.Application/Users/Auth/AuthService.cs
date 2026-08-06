using Kela.Application.Repositories;
using Kela.Application.Users.Auth.Requests;
using Kela.Application.Users.Auth.Responses;
using Kela.Application.Validation;
using Kela.Domain.Enums;

namespace Kela.Application.Users.Auth;

internal sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IValidator<LoginRequest> loginValidator) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        loginValidator.Validate(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(normalizedEmail, cancellationToken);

        // Kullanıcı yoksa da aynı sonucu dön → user enumeration koruması
        if (user is null
            || user.Status != UserStatus.Active
            || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return new LoginResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Role);
    }
}
