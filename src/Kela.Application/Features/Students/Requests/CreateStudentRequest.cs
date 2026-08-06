namespace Kela.Application.Features.Students.Requests;

/// <summary>
/// Öğretmen/admin tarafından yeni öğrenci oluşturma isteği.
/// Kimlik & login bilgileri User'a, öğrenciye özgü bilgiler StudentProfile'a yazılır.
/// </summary>
public sealed record CreateStudentRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    DateTime? BirthDate,
    int? CityId);
