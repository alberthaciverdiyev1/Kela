namespace Kela.Application.Features.Students.Requests;

/// <summary>Öğrenci güncelleme isteği — ad/soyad/telefon/şehir/doğum tarihi.</summary>
public sealed record UpdateStudentRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? BirthDate,
    int? CityId);
