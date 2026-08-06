using Kela.Domain.Common;

namespace Kela.Application.Features.Users.Auth.Responses;

public sealed record LoginResponse(
    int UserId,
    string FirstName,
    string LastName,
    string Role);
