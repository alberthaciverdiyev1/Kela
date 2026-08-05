using Kela.Domain.Users.Enums;

namespace Kela.Application.Users.Dtos;

public sealed record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    Role Role,
    UserStatus Status,
    DateTime CreatedAt);
