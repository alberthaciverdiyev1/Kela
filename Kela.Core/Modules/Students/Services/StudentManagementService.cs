using ClosedXML.Excel;
using Kela.Core.Common.Bulk;
using Kela.Core.Common.Email;
using Kela.Core.Common.Responses;
using Kela.Core.Data;
using Kela.Core.Modules.Students.Requests;
using Kela.Core.Modules.Students.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Students.Services;

using Student = Kela.Core.Modules.Auth.Entities.Student;

public sealed class StudentManagementService(AppDbContext db, IEmailSender email) : Abstractions.IStudentManagementService
{
    public async Task<PagedResponse<StudentListItemResponse>> ListAsync(StudentListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage < 1 ? 20 : Math.Min(request.PerPage, 500);

        var query = Query(request);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        return new PagedResponse<StudentListItemResponse>(
            items.Select(ToResponse).ToList(), page, LastPage(total, perPage), perPage, total);
    }

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

    public async Task<byte[]> ExportXlsxAsync(StudentListRequest request, CancellationToken ct = default)
    {
        var students = await Query(request).OrderBy(x => x.Id).ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Students");
        var headers = new[] { "Id", "FullName", "Email", "Phone", "IsActive", "CreatedAt" };
        for (var c = 0; c < headers.Length; c++) ws.Cell(1, c + 1).Value = headers[c];
        ws.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var s in students)
        {
            ws.Cell(row, 1).Value = s.Id;
            ws.Cell(row, 2).Value = s.FullName;
            ws.Cell(row, 3).Value = s.Email;
            ws.Cell(row, 4).Value = s.Phone ?? "";
            ws.Cell(row, 5).Value = s.IsActive;
            ws.Cell(row, 6).Value = s.CreatedAt.ToString("O");
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<GenerateStudentsResponse> GenerateAsync(GenerateStudentsRequest request, CancellationToken ct = default)
    {
        var count = request.Count is < 1 or > 1000 ? 10 : request.Count;
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var domain = string.IsNullOrWhiteSpace(request.EmailDomain) ? "generated.local" : request.EmailDomain.Trim().TrimStart('@');

        var created = new List<Student>(count);
        for (var i = 1; i <= count; i++)
        {
            var student = new Student
            {
                FullName = $"{request.FullNamePrefix} {i}",
                Email = $"{request.EmailPrefix}{i}@{domain}".ToLowerInvariant(),
                PasswordHash = hash,
                IsActive = true
            };
            db.Students.Add(student);
            created.Add(student);
        }

        await db.SaveChangesAsync(ct);

        if (request.GroupId is long groupId)
        {
            foreach (var student in created)
            {
                db.StudentGroups.Add(new Modules.Group.Entities.StudentGroup
                {
                    GroupId = groupId,
                    StudentId = student.Id
                });
            }
            await db.SaveChangesAsync(ct);
        }

        var sent = false;
        if (request.SendCredentials)
        {
            foreach (var student in created)
            {
                try
                {
                    await email.SendAsync(student.Email, "Hesab məlumatlarınız",
                        $"<p>Salam, {student.FullName}.</p><p>E-poçt: <b>{student.Email}</b><br/>Şifrə: <b>{request.Password}</b></p>", ct);
                }
                catch { /* SMTP konfiqurasiya olunmayıb */ }
            }
            sent = true;
        }

        return new GenerateStudentsResponse(count, sent,
            created.Select(s => new GeneratedStudentInfo(s.Id, s.FullName, s.Email)).ToList());
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

    private IQueryable<Student> Query(StudentListRequest request)
    {
        var query = db.Students.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters(new[] { "SoftDelete" }).Where(x => x.DeletedAt != null);
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

    private static int LastPage(int total, int perPage) => (int)Math.Ceiling(total / (double)(perPage < 1 ? 20 : perPage));

    private static StudentListItemResponse ToResponse(Student s) =>
        new(s.Id, s.FullName, s.Email, s.Phone, s.IsActive, s.CreatedAt);
}
