using Microsoft.AspNetCore.Http;
using Kela.Core.Common.Exceptions;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Auth.Entities;
using Kela.Core.Modules.Auth.Requests;
using Kela.Core.Modules.Auth.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kela.Core.Modules.Auth.Services;

public sealed class AuthService(
    AppDbContext db,
    ITokenService tokens,
    IOtpService otp,
    IStudentDeviceService devices,
    IFileStorage storage,
    ITenantProvider tenantProvider,
    IConfiguration config) : IAuthService
{
    private bool StrictTenant => config.GetValue<bool>("Tenancy:Strict");

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = Normalize(request.Email);
        var tenantId = ResolveTenant(request.TenantId);

        if (await db.Students.IgnoreQueryFilters()
                .AnyAsync(x => x.TenantId == tenantId && x.Email == email && x.DeletedAt == null, ct))
        {
            throw new ArgumentException("A user with this email already exists");
        }

        if (!string.IsNullOrWhiteSpace(request.Phone) &&
            await db.Students.IgnoreQueryFilters()
                .AnyAsync(x => x.TenantId == tenantId && x.Phone == request.Phone && x.DeletedAt == null, ct))
        {
            throw new ArgumentException("A user with this phone already exists");
        }

        var student = new Student
        {
            TenantId = tenantId,
            FullName = request.FullName,
            Email = email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);

        return new AuthResponse(tokens.CreateToken(student.Id, student.Email, student.FullName, AuthRoles.Student, student.TenantId), ToResponse(student));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = Normalize(request.Email);
        var tenantId = request.TenantId ?? tenantProvider.TenantId;

        if (tenantId is null && StrictTenant)
        {
            throw new UnauthorizedAccessException("Tenant is required");
        }

        if (tenantId is not null)
        {
            await EnsureTenantActiveAsync(tenantId.Value, ct);
        }

        var student = tenantId is long tid
            ? await db.Students.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TenantId == tid && x.Email == email && x.DeletedAt == null, ct)
            : await db.Students.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null, ct);

        if (student is null || !BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash))
        {
            throw new UnauthorizedAccessException("Email or password is incorrect");
        }

        if (tenantId is not null && student.TenantId != tenantId.Value)
        {
            throw new UnauthorizedAccessException("Email or password is incorrect");
        }

        if (!student.IsActive)
        {
            throw new UnauthorizedAccessException("User is not active");
        }

        if (!string.IsNullOrWhiteSpace(request.DeviceId) &&
            !await devices.HasAccessAsync(student.Id, request.DeviceId, ct))
        {
            throw new ForbiddenException("This account is active on a different device");
        }

        return new AuthResponse(tokens.CreateToken(student.Id, student.Email, student.FullName, AuthRoles.Student, student.TenantId), ToResponse(student));
    }

    private long ResolveTenant(long? requested)
    {
        var tenantId = requested ?? tenantProvider.TenantId;
        if (tenantId is null)
        {
            if (StrictTenant) throw new ArgumentException("Tenant is required");
            return 0; // non-strict (legacy/test): qlobal tenant
        }

        return tenantId.Value;
    }

    private async Task EnsureTenantActiveAsync(long tenantId, CancellationToken ct)
    {
        if (!await db.Tenants.AnyAsync(t => t.Id == tenantId && t.IsActive, ct))
        {
            throw new UnauthorizedAccessException("Tenant is not active");
        }
    }

    public async Task<StudentResponse> GetProfileAsync(long studentId, CancellationToken ct = default)
        => ToResponse(await GetStudentAsync(studentId, ct));

    public Task SendOtpAsync(SendOtpRequest request, CancellationToken ct = default)
        => otp.SendAsync(request.Email, ct);

    public Task<bool> CheckOtpAsync(CheckOtpRequest request, CancellationToken ct = default)
        => otp.VerifyAsync(request.Email, request.Otp, consume: false, ct);

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        if (!await otp.VerifyAsync(request.Email, request.Otp, consume: true, ct))
        {
            throw new ArgumentException("Invalid or expired OTP");
        }

        var email = Normalize(request.Email);
        var student = await db.Students.FirstOrDefaultAsync(x => x.Email == email, ct)
            ?? throw new KeyNotFoundException("Student not found");

        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await db.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(long studentId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var student = await GetStudentAsync(studentId, ct);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, student.PasswordHash))
        {
            throw new ArgumentException("Current password is incorrect");
        }

        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await db.SaveChangesAsync(ct);
    }

    public async Task<StudentResponse> ChangeEmailAsync(long studentId, ChangeEmailRequest request, CancellationToken ct = default)
    {
        if (!await otp.VerifyAsync(request.Email, request.Otp, consume: true, ct))
        {
            throw new ArgumentException("Invalid or expired OTP");
        }

        var student = await GetStudentAsync(studentId, ct);
        var email = Normalize(request.Email);

        if (await db.Students.AnyAsync(x => x.Email == email && x.Id != studentId, ct))
        {
            throw new ArgumentException("Email already in use");
        }

        student.Email = email;
        await db.SaveChangesAsync(ct);
        return ToResponse(student);
    }

    public async Task<StudentResponse> ChangePhoneAsync(long studentId, ChangePhoneRequest request, CancellationToken ct = default)
    {
        var student = await GetStudentAsync(studentId, ct);
        student.Phone = request.Phone;
        await db.SaveChangesAsync(ct);
        return ToResponse(student);
    }

    public async Task<StudentResponse> ChangeFullNameAsync(long studentId, ChangeFullNameRequest request, CancellationToken ct = default)
    {
        var student = await GetStudentAsync(studentId, ct);
        student.FullName = request.FullName;
        await db.SaveChangesAsync(ct);
        return ToResponse(student);
    }

    public async Task<StudentResponse> ChangeImageAsync(long studentId, IFormFile file, CancellationToken ct = default)
    {
        var student = await GetStudentAsync(studentId, ct);
        var path = await storage.UploadAsync(file, "student", ct);
        student.Image = path;
        await db.SaveChangesAsync(ct);
        return ToResponse(student);
    }

    public async Task<StudentResponse> SetNotificationStatusAsync(long studentId, NotificationStatusRequest request, CancellationToken ct = default)
    {
        var student = await GetStudentAsync(studentId, ct);
        student.NotificationStatus = request.Enabled;
        await db.SaveChangesAsync(ct);
        return ToResponse(student);
    }

    public async Task DeleteAccountAsync(long studentId, CancellationToken ct = default)
    {
        var student = await GetStudentAsync(studentId, ct);
        student.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task<Student> GetStudentAsync(long id, CancellationToken ct)
        => await db.Students.IgnoreQueryFilters(new[] { "SoftDelete" }).FirstOrDefaultAsync(x => x.Id == id, ct)
           ?? throw new KeyNotFoundException($"Student not found: {id}");

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    private StudentResponse ToResponse(Student student) =>
        new(student.Id, student.FullName, student.Email, student.Phone, storage.GetUrl(student.Image));
}
