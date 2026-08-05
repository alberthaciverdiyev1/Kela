using Kela.Application.Abstractions.Cqrs;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    int Id,
    string FirstName,
    string LastName,
    string? Password,
    UserStatus? Status) : ICommand;
