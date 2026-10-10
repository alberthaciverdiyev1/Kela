using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Auth.Entities;
using LanguageLMS.Core.Modules.Auth.Requests;
using LanguageLMS.Core.Modules.Auth.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Auth.Services;

public sealed class AuthService(
    AppDbContext db,
    ITokenService tokens,
    IFileStorage storage) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Students.AnyAsync(x => x.Email == email, ct))
        {
            throw new ArgumentException("A user with this email already exists");
        }

        if (!string.IsNullOrWhiteSpace(request.Phone) &&
            await db.Students.AnyAsync(x => x.Phone == request.Phone, ct))
        {
            throw new ArgumentException("A user with this phone already exists");
        }

        var student = new Student
        {
            FullName = request.FullName,
            Email = email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);

        return new AuthResponse(tokens.CreateToken(student.Id, student.Email, student.FullName, AuthActors.Student), ToResponse(student));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var student = await db.Students.FirstOrDefaultAsync(x => x.Email == email, ct);

        if (student is null || !BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash))
        {
            throw new UnauthorizedAccessException("Email or password is incorrect");
        }

        if (!student.IsActive)
        {
            throw new UnauthorizedAccessException("User is not active");
        }

        return new AuthResponse(tokens.CreateToken(student.Id, student.Email, student.FullName, AuthActors.Student), ToResponse(student));
    }

    public async Task<StudentResponse> GetProfileAsync(long studentId, CancellationToken ct = default)
    {
        var student = await db.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Id == studentId, ct)
            ?? throw new KeyNotFoundException($"Student not found: {studentId}");

        return ToResponse(student);
    }

    private StudentResponse ToResponse(Student student) =>
        new(student.Id, student.FullName, student.Email, student.Phone, storage.GetUrl(student.Image));
}
