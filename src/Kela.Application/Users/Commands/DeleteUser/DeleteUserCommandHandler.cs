using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Repositories;

namespace Kela.Application.Users.Commands.DeleteUser;

internal sealed class DeleteUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {command.Id} olan kullanıcı bulunamadı.");

        // Soft delete
        user.DeletedAt = DateTime.UtcNow;
        user.SetStatus(Kela.Domain.Users.Enums.UserStatus.Inactive);

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
