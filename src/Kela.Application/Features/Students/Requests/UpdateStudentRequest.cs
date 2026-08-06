namespace Kela.Application.Features.Students.Requests;

public sealed record UpdateStudentRequest(
    string FirstName,
    string? LastName,
    string? PhoneNumber,
    DateOnly? BirthDate,
    int? CityId);
