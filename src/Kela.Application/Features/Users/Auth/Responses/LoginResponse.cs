using Kela.Domain.Common;

namespace Kela.Application.Features.Users.Auth.Responses;

/// <summary>
/// Başarılı login sonucu. <see cref="Role"/> artık Identity rol adıdır (string):
/// "Admin", "Teacher", "Student" veya "Parent". Endpoint cookie claim'lerini üretir.
/// </summary>
public sealed record LoginResponse(
    int UserId,
    string FirstName,
    string LastName,
    string Role);
