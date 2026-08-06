namespace Kela.Application.Features.Students.Responses;

public sealed record StudentResponse(
    int Id,
    int UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Email,
    DateOnly? BirthDate,
    int? CityId,
    string? CityName,
    DateTime CreatedAt);
