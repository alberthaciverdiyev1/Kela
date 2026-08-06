using Kela.Domain.Enums;

namespace Kela.Application.Users.Responses;

/// <summary>Kullanıcı bilgisi (yalnızca okuma amaçlı yanıt).</summary>
public sealed record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    Role Role,
    UserStatus Status,
    DateTime CreatedAt);
