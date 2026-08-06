namespace Kela.Application.Features.Students.Responses;

/// <summary>
/// Öğrenci yanıtı — User kimlik bilgileri + StudentProfile öğrenci bilgileri.
/// <see cref="CityName"/> istenen dile göre yerelleştirilmiş şehir adıdır.
/// </summary>
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
