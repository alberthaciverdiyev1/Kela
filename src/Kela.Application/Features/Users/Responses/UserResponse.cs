using Kela.Domain.Common;
using Kela.Domain.Enums;

namespace Kela.Application.Features.Users.Responses;

public sealed record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    UserStatus Status,
    DateTime CreatedAt);
