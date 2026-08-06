using Kela.Domain.Enums;

namespace Kela.Application.Users.Auth;

/// <summary>
/// Başarılı login sonucu. Controller bu bilgilerle cookie claim'lerini üretir.
/// </summary>
public sealed record LoginResult(
    int UserId,
    string FirstName,
    string LastName,
    Role Role);
