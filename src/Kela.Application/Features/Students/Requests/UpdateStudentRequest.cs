namespace Kela.Application.Features.Students.Requests;

/// <summary>Öğrenci güncelleme isteği — ad/soyad/şehir/doğum tarihi.</summary>
public sealed record UpdateStudentRequest(
    string FirstName,
    string LastName,
    DateTime? BirthDate,
    int? CityId);
