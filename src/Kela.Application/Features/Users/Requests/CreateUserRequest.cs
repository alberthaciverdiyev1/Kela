using Kela.Domain.Common;

namespace Kela.Application.Features.Users.Requests;

/// <summary>
/// Yeni kullanıcı oluşturma isteği. Role, Identity rol adlarından biri olmalıdır
/// (bkz. <see cref="RoleNames"/>): "Admin", "Teacher", "Student", "Parent".
/// </summary>
public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    string? PhoneNumber = null);
