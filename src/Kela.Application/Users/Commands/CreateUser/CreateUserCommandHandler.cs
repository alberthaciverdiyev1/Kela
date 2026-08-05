using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Abstractions.Security;
using Kela.Application.Repositories;
using Kela.Domain.Users;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Users.Commands.CreateUser;

internal sealed class CreateUserCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher)
    : ICommandHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        if (await users.EmailExistsAsync(email, cancellationToken))
        {
            throw new InvalidOperationException($"'{email}' email adresi zaten kayıtlı.");
        }

        var user = new User(command.FirstName.Trim(), command.LastName.Trim(), email)
        {
            CreatedAt = DateTime.UtcNow,
        };

        user.SetPasswordHash(passwordHasher.Hash(command.Password));
        // Rol ↔ profil tutarlılığını domain garantiler: yalnızca role uyan tek profil kurulur.
        user.AssignProfile(command.Role);

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
