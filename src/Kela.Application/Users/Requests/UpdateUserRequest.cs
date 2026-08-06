using Kela.Domain.Enums;

namespace Kela.Application.Users.Requests;

/// <summary>Mevcut kullanıcıyı güncelleme isteği.</summary>
public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string? Password,
    UserStatus? Status);
