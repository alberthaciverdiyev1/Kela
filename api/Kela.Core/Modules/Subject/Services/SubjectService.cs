using Kela.Core.Common.Exceptions;
using Kela.Core.Common.Sorting;
using Kela.Core.Common.Pagination;
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

        var query = db.Subjects.AsNoTracking();

        if (request.IsDeleted == 1)
        {
            query = query.IgnoreQueryFilters(new[] { "SoftDelete" }).Where(x => x.DeletedAt != null);
        }

        if (request.UserId.HasValue)
        {
            // Teacher yalnız özü yaratdığını görür; sahibi olmayan (admin) fənnlər hamıya açıqdır.
            query = query.Where(x => x.UserId == request.UserId || x.UserId == null);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search}%"));
        }

        return await query
            .ApplyListSort(request, x => x.Name)
        .ToPagedAsync(request, ToResponse, ct);
    }

    public async Task<SubjectResponse> GetAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var category = await db.Subjects
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Subject not found: {id}");

        EnsureAccessible(category, ownerId);
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

    public async Task<SubjectResponse> UpdateAsync(long id, SubjectUpsertRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        var category = await db.Subjects
            .IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Subject not found: {id}");

        EnsureOwned(category, ownerId);
        await EnsureNameAvailableAsync(request.Name, id, ct);

        // Sahiblik yaratma anında qeyd olunur — redaktə onu dəyişmir.
        category.Name = request.Name;
        await db.SaveChangesAsync(ct);

        return ToResponse(category);
    }

    public async Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var category = await db.Subjects.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Subject not found: {id}");

        EnsureOwned(category, ownerId);

        category.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureNameAvailableAsync(string name, long? excludeId, CancellationToken ct)
    {
        var exists = await db.Subjects.IgnoreQueryFilters(new[] { "SoftDelete" })
            .AnyAsync(x => x.Name == name && (excludeId == null || x.Id != excludeId), ct);

        if (exists)
        {
            throw new ArgumentException("A main category with this name already exists");
        }
    }

    private static SubjectResponse ToResponse(Subject category) =>
        new(category.Id, category.Name, category.UserId);

    public async Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default)
    {
        if (ownerId.HasValue)
        {
            var owned = await db.Subjects
                .Where(x => request.Ids.Contains(x.Id) && x.UserId == ownerId)
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (owned.Count != request.Ids.Distinct().Count())
            {
                throw new ForbiddenException("You can only modify your own subjects");
            }
        }

        await BulkHelper.ApplyAsync<Subject>(db, request, ct);
    }

    /// <summary>Oxumaq üçün: öz qeydi və ya admin tərəfindən yaradılan (sahibsiz) qeyd.</summary>
    private static void EnsureAccessible(Subject subject, long? ownerId)
    {
        if (ownerId.HasValue && subject.UserId != ownerId && subject.UserId != null)
        {
            throw new ForbiddenException("You do not have access to this subject");
        }
    }

    /// <summary>Dəyişmək üçün yalnız sahibi olduğu qeyd.</summary>
    private static void EnsureOwned(Subject subject, long? ownerId)
    {
        if (ownerId.HasValue && subject.UserId != ownerId)
        {
            throw new ForbiddenException("You can only modify your own subject");
        }
    }
}
