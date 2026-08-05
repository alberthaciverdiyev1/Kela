using Kela.Domain.Users.Enums;

namespace Kela.Application.Users.Commands.Login;

/// <summary>
/// Başarılı login sonucu. Controller bu bilgilerle cookie claim'lerini üretir.
/// </summary>
public sealed record LoginResult(
    int UserId,
    int TenantId,
    string FirstName,
    string LastName,
    Role Role);
