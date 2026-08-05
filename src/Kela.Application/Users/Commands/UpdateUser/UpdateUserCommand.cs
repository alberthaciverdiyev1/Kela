using System.ComponentModel.DataAnnotations;
using Kela.Application.Abstractions.Cqrs;
using Kela.Domain.Users.Enums;

namespace Kela.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    int Id,
    [property: Required, StringLength(100)] string FirstName,
    [property: Required, StringLength(100)] string LastName,
    [property: StringLength(128, MinimumLength = 6)] string? Password,
    [property: EnumDataType(typeof(UserStatus))] UserStatus? Status) : ICommand;
