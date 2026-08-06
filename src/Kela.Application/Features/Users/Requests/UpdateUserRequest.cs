using Kela.Domain.Enums;

namespace Kela.Application.Features.Users.Requests;

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string? Password,
    UserStatus? Status);
