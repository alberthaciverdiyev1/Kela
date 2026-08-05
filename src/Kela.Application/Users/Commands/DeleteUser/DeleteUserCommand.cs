using Kela.Application.Abstractions.Cqrs;

namespace Kela.Application.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(int Id) : ICommand;
