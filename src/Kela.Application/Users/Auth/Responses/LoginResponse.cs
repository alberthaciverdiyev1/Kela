using Kela.Domain.Enums;

namespace Kela.Application.Users.Auth.Responses;

/// <summary>Başarılı login sonucu. Endpoint bu bilgilerle cookie claim'lerini üretir.</summary>
public sealed record LoginResponse(
    int UserId,
    string FirstName,
    string LastName,
    Role Role);
