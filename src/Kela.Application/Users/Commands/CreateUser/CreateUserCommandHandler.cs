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

        var user = new User
        {
            Id = 0, // EF identity DB'de üretir
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            Email = email,
            Password = passwordHasher.Hash(command.Password),
            Role = command.Role,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };

        switch (command.Role)
        {
            case Role.Teacher:
                user.Teacher = new Teacher
                {
                    Id = 0
                };
                break;

            case Role.Student:
                user.Student = new Student
                {
                    Id = 0,
                };
                break;

            case Role.Parent:
                user.Parent = new Parent
                {
                    Id = 0,
                };
                break;
        }

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
