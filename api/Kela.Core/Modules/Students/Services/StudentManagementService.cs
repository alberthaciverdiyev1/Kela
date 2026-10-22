using Kela.Core.Common.Sorting;
using System.Security.Cryptography;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Pagination;
using Kela.Core.Common.Responses;
using Kela.Core.Data;
using Kela.Core.Modules.Students.Requests;
using Kela.Core.Modules.Students.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Students.Services;

using Student = Kela.Core.Modules.Auth.Entities.Student;

public sealed class StudentManagementService(
    AppDbContext db,
    Kela.Core.Modules.Subscription.Abstractions.ISubscriptionService subscription)
    : Abstractions.IStudentManagementService
{
    public async Task<PagedResponse<StudentListItemResponse>> ListAsync(StudentListRequest request, CancellationToken ct = default)
        => await Query(request)
            .ApplyListSort(request, x => x.FullName)
            .ToPagedAsync(request, ToResponse, ct);

    public async Task<StudentListItemResponse> GetAsync(long id, CancellationToken ct = default)
        => ToResponse(await db.Students.IgnoreQueryFilters(new[] { "SoftDelete" }).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Student not found: {id}"));

    public async Task<StudentListItemResponse> SetStatusAsync(long id, SetStudentStatusRequest request, CancellationToken ct = default)
    {
        var student = await db.Students.IgnoreQueryFilters(new[] { "SoftDelete" }).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Student not found: {id}");

        student.IsActive = request.IsActive;
        await db.SaveChangesAsync(ct);
        return ToResponse(student);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Student not found: {id}");
        student.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(long id, CancellationToken ct = default)
    {
        var student = await db.Students.IgnoreQueryFilters(new[] { "SoftDelete" }).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Student not found: {id}");
        student.DeletedAt = null;
        await db.SaveChangesAsync(ct);
    }

    public async Task<ResetStudentPasswordResponse> ResetPasswordAsync(long id, ResetStudentPasswordRequest request, CancellationToken ct = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Student not found: {id}");

        var generated = string.IsNullOrWhiteSpace(request.Password);
        var password = generated ? GeneratePassword() : request.Password!.Trim();

        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        // Başqası tərəfindən verilən şifrə ilk girişdə dəyişdirilməlidir.
        student.MustChangePassword = true;
        await db.SaveChangesAsync(ct);

        return new ResetStudentPasswordResponse(student.Id, student.Email, generated ? password : null);
    }

    public async Task<CreateStudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken ct = default)
    {
        await subscription.EnsureCanAddStudentsAsync(1, ct);

        var fullName = (request.FullName ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

        if (fullName.Length == 0) throw new ArgumentException("Ad və soyad tələb olunur");
        if (email.Length == 0) throw new ArgumentException("E-poçt tələb olunur");

        var exists = await db.Students.IgnoreQueryFilters(new[] { "SoftDelete" })
            .AnyAsync(s => s.Email == email, ct);
        if (exists) throw new ArgumentException("Bu e-poçt ilə şagird artıq mövcuddur");

        var generated = string.IsNullOrWhiteSpace(request.Password);
        var password = generated ? GeneratePassword() : request.Password!.Trim();

        var student = new Student
        {
            FullName = fullName,
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = request.IsActive,
            // Təsadüfi şifrə verilibsə ilk girişdə dəyişməlidir; ad realdır.
            MustChangePassword = generated
        };

        db.Students.Add(student);
        await db.SaveChangesAsync(ct);

        if (request.GroupId is long groupId)
        {
            db.StudentGroups.Add(new Modules.Group.Entities.StudentGroup
            {
                GroupId = groupId,
                StudentId = student.Id
            });
            await db.SaveChangesAsync(ct);
        }

        return new CreateStudentResponse(student.Id, student.FullName, student.Email, generated ? password : null);
    }

    public async Task<IReadOnlyList<StudentDeviceResponse>> ListDevicesAsync(long studentId, CancellationToken ct = default)
        => await db.StudentDevices.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.Id)
            .Select(x => new StudentDeviceResponse(x.Id, x.DeviceId, x.CreatedAt))
            .ToListAsync(ct);

    public async Task UnbindDeviceAsync(long studentId, CancellationToken ct = default)
    {
        var devices = await db.StudentDevices.Where(x => x.StudentId == studentId).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var d in devices) d.DeletedAt = now;
        await db.SaveChangesAsync(ct);
    }

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<Student>(db, request, ct);

    public Task<bool> IsInTeacherGroupsAsync(long studentId, long teacherId, CancellationToken ct = default)
        => db.StudentGroups.AnyAsync(
            sg => sg.StudentId == studentId
                  && db.Groups.Any(g => g.Id == sg.GroupId && g.UserId == teacherId), ct);

    private IQueryable<Student> Query(StudentListRequest request)
    {
        var query = db.Students.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters(new[] { "SoftDelete" }).Where(x => x.DeletedAt != null);
        }

        if (request.OwnerId is long ownerId)
        {
            var studentIds = db.StudentGroups
                .Where(sg => db.Groups.Any(g => g.Id == sg.GroupId && g.UserId == ownerId))
                .Select(sg => sg.StudentId);

            query = query.Where(x => studentIds.Contains(x.Id));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive);
        }

        if (request.GroupId.HasValue)
        {
            query = query.Where(s => db.StudentGroups.Any(sg => sg.StudentId == s.Id && sg.GroupId == request.GroupId));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.FullName, $"%{search}%")
                || EF.Functions.ILike(x.Email, $"%{search}%")
                || (x.Phone != null && EF.Functions.ILike(x.Phone, $"%{search}%")));
        }

        return query;
    }


    private static StudentListItemResponse ToResponse(Student s) =>
        new(s.Id, s.FullName, s.Email, s.Phone, s.IsActive, s.CreatedAt);

    /// <summary>Təsadüfi güclü şifrə (12 simvol: hərf + rəqəm, oxşar simvollar istisna).</summary>
    private static string GeneratePassword()
    {
        const string letters = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
        const string digits = "23456789";
        var chars = new char[12];
        for (var i = 0; i < chars.Length; i++)
        {
            var pool = i % 3 == 2 ? digits : letters;
            chars[i] = pool[RandomNumberGenerator.GetInt32(pool.Length)];
        }
        // ən azı bir hərf və bir rəqəm zəmanətli olsun
        chars[0] = letters[RandomNumberGenerator.GetInt32(letters.Length)];
        chars[^1] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        return new string(chars);
    }
}
