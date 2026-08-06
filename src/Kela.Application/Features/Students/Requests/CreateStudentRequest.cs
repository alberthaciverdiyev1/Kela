namespace Kela.Application.Features.Students.Requests;

/// <summary>
/// Öğretmen tarafından yeni öğrenci oluşturma isteği.
/// Mail ve şifre GİRİLMEZ — sistem bunları rastgele üretir ve oluşturma
/// yanıtında geri verir (bkz. StudentCreatedResponse).
/// </summary>
public sealed record CreateStudentRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? BirthDate,
    int? CityId);
