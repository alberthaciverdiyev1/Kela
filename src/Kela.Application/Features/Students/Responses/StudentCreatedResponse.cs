namespace Kela.Application.Features.Students.Responses;

/// <summary>
/// Öğrenci oluşturma sonucu. Sistem tarafından üretilen <see cref="Email"/> ve
/// <see cref="Password"/> burada düz metin olarak DÖNER — öğretmenin öğrenciye
/// iletmesi için. Şifre yalnızca bu an döner; veritabanında hash'li saklanır.
/// </summary>
public sealed record StudentCreatedResponse(
    int Id,
    int UserId,
    string Email,
    string Password,
    DateTime CreatedAt);
