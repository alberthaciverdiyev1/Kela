using System.ComponentModel.DataAnnotations;
using Kela.Application.Abstractions.Cqrs;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    [property: Required, StringLength(100)] string FirstName,
    [property: Required, StringLength(100)] string LastName,
    [property: Required, EmailAddress, StringLength(255)] string Email,
    [property: Required, StringLength(128, MinimumLength = 6)] string Password,
    [property: Required] Role Role) : ICommand<int>;
