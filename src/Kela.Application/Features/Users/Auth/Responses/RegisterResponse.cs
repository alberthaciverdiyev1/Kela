namespace Kela.Application.Features.Users.Auth.Responses;

/// <summary>Başarılı kayıt sonucu. Daima Teacher rolünde oluşturulur.</summary>
public sealed record RegisterResponse(
    int UserId,
    string FirstName,
    string LastName,
    string Email);
