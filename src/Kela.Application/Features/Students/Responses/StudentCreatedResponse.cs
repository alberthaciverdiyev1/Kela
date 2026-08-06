namespace Kela.Application.Features.Students.Responses;

public sealed record StudentCreatedResponse(
    int Id,
    int UserId,
    string Email,
    string Password,
    DateTime CreatedAt);
