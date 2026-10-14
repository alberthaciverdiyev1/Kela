using Kela.Core.Common.Responses;
using Kela.Core.Common.Bulk;
using Kela.Core.Data;
using Kela.Core.Modules.Subject.Requests;
using Kela.Core.Modules.Subject.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Subject.Services;

using Subject = Kela.Core.Modules.Subject.Entities.Subject;

public sealed class SubjectService(AppDbContext db) : ISubjectService
{
    public async Task<PagedResponse<SubjectResponse>> ListAsync(SubjectListRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var perPage = request.PerPage is < 1 or > 200 ? 20 : request.PerPage;

        var query = db.Subjects.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters().Where(x => x.DeletedAt != null);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search}%"));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        var data = items.Select(ToResponse).ToList();
        var lastPage = (int)Math.Ceiling(total / (double)perPage);

        return new PagedResponse<SubjectResponse>(data, page, lastPage, perPage, total);
    }

    public async Task<SubjectResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var category = await db.Subjects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Subject not found: {id}");

        return ToResponse(category);
    }

    public async Task<SubjectResponse> CreateAsync(SubjectUpsertRequest request, CancellationToken ct = default)
    {
        await EnsureNameAvailableAsync(request.Name, null, ct);

        var category = new Subject
        {
            Name = request.Name,
            UserId = request.UserId
        };

        db.Subjects.Add(category);
        await db.SaveChangesAsync(ct);

        return ToResponse(category);
    }

    public async Task<SubjectResponse> UpdateAsync(long id, SubjectUpsertRequest request, CancellationToken ct = default)
    {
        var category = await db.Subjects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Subject not found: {id}");

        await EnsureNameAvailableAsync(request.Name, id, ct);

        category.Name = request.Name;
        category.UserId = request.UserId;
        await db.SaveChangesAsync(ct);

        return ToResponse(category);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var category = await db.Subjects.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Subject not found: {id}");

        category.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureNameAvailableAsync(string name, long? excludeId, CancellationToken ct)
    {
        var exists = await db.Subjects.IgnoreQueryFilters()
            .AnyAsync(x => x.Name == name && (excludeId == null || x.Id != excludeId), ct);

        if (exists)
        {
            throw new ArgumentException("A main category with this name already exists");
        }
    }

    private static SubjectResponse ToResponse(Subject category) =>
        new(category.Id, category.Name, category.UserId);

    public Task BulkAsync(BulkRequest request, CancellationToken ct = default)
        => BulkHelper.ApplyAsync<Subject>(db, request, ct);
}
