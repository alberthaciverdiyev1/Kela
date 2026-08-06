using Kela.Domain.Enums;

namespace Kela.Application.Features.Users.Requests;

/// <summary>Yeni kullanıcı oluşturma isteği.</summary>
public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    Role Role);
