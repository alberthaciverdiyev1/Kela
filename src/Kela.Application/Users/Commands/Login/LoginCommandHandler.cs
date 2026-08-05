using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Abstractions.Security;
using Kela.Application.Repositories;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Users.Commands.Login;

internal sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher)
    : ICommandHandler<LoginCommand, LoginResult?>
{
    public async Task<LoginResult?> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(email, cancellationToken);

        // Kullanıcı yoksa da aynı sonucu dön → user enumeration koruması
        if (user is null
            || user.Status != UserStatus.Active
            || !passwordHasher.Verify(command.Password, user.Password))
        {
            return null;
        }

        return new LoginResult(
            user.Id,
            user.TenantId,
            user.FirstName,
            user.LastName,
            user.Role);
    }
}
