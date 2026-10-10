using System.Text;
using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Students.Requests;
using LanguageLMS.Core.Modules.Students.Responses;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Modules.Students.Services;

using Student = LanguageLMS.Core.Modules.Auth.Entities.Student;

public sealed class StudentManagementService(AppDbContext db) : Abstractions.IStudentManagementService
{
    public async Task<PagedResponse<StudentListItemResponse>> ListAsync(StudentListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 200 ? 20 : request.PerPage;

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
        => ToResponse(await db.Students.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Student not found: {id}"));

    public async Task<StudentListItemResponse> SetStatusAsync(long id, SetStudentStatusRequest request, CancellationToken ct = default)
    {
        var student = await db.Students.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct)
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
        var student = await db.Students.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Student not found: {id}");
        student.DeletedAt = null;
        await db.SaveChangesAsync(ct);
    }

    public async Task<byte[]> ExportCsvAsync(StudentListRequest request, CancellationToken ct = default)
    {
        var students = await Query(request).OrderBy(x => x.Id).ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Id,FullName,Email,Phone,IsActive,CreatedAt");
        foreach (var s in students)
        {
            sb.AppendLine($"{s.Id},\"{s.FullName}\",\"{s.Email}\",\"{s.Phone}\",{s.IsActive},{s.CreatedAt:O}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<Student>(db, request, ct);

    private IQueryable<Student> Query(StudentListRequest request)
    {
        var query = db.Students.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
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
