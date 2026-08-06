using Kela.Domain.Common;

namespace Kela.Application.Features.Users.Requests;

public sealed record CreateUserRequest(
    string FirstName,
    string? LastName,
    string Email,
    string Password,
    string Role,
    string? PhoneNumber = null);
