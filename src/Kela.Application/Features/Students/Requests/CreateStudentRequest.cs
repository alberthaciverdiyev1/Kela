namespace Kela.Application.Features.Students.Requests;

public sealed record CreateStudentRequest(
    string FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Email,
    DateOnly? BirthDate,
    int? CityId);
