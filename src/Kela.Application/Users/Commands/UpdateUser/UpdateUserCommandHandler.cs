using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Abstractions.Security;
using Kela.Application.Repositories;

namespace Kela.Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher)
    : ICommandHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {command.Id} olan kullanıcı bulunamadı.");

        user.SetName(command.FirstName, command.LastName);

        if (!string.IsNullOrWhiteSpace(command.Password))
        {
            user.SetPasswordHash(passwordHasher.Hash(command.Password));
        }

        if (command.Status is not null)
        {
            user.SetStatus(command.Status.Value);
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
