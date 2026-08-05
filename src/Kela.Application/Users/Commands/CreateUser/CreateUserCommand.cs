using Kela.Application.Abstractions.Cqrs;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    Role Role) : ICommand<int>;
