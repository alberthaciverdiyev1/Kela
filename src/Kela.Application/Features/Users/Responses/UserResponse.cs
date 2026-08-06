using Kela.Domain.Common;
using Kela.Domain.Enums;

namespace Kela.Application.Features.Users.Responses;

/// <summary>
/// Kullanıcı bilgisi (yalnızca okuma amaçlı yanıt).
/// <see cref="Role"/>, Identity rol adıdır (string): "Admin", "Teacher", "Student", "Parent".
/// </summary>
public sealed record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    UserStatus Status,
    DateTime CreatedAt);
