namespace Kela.Application.Features.Users.Auth.Responses;

public sealed record RegisterResponse(
    int UserId,
    string FirstName,
    string LastName,
    string Email);
