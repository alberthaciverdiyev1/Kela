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
using Kela.Core.Common.Queries;
using Kela.Core.Common.Authorization;

public sealed class SubjectService(
    AppDbContext db,
    Kela.Core.Modules.Subscription.Abstractions.ISubscriptionService subscription) : ISubjectService
{
    public async Task<PagedResponse<SubjectResponse>> ListAsync(SubjectListRequest request, CancellationToken ct = default)
    {

        var query = db.Subjects.AsNoTracking();

        query = query.ApplyTrashFilter(request.IsDeleted);

        if (request.UserId is long ownerId)
        {
            // Teacher öz fənnini və bütün müəllimlərə paylaşılan fənnləri görür.
            query = query.Where(x => x.UserId == ownerId || x.IsShared);
        }

        query = query.SearchBy(request.Search, x => x.Name);

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

        OwnershipGuard.EnsureReadable(ownerId, category.UserId, category.IsShared, "You do not have access to this subject");
        return ToResponse(category);
    }

    public async Task<SubjectResponse> CreateAsync(SubjectUpsertRequest request, CancellationToken ct = default)
    {
        await subscription.EnsureCanAddSubjectsAsync(1, ct);

        await EnsureNameAvailableAsync(request.Name, null, ct);

        var category = new Subject
        {
            Name = request.Name,
            UserId = request.UserId,
            IsShared = request.IsShared
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

        OwnershipGuard.EnsureOwned(ownerId, category.UserId, "You can only modify your own subject");
        await EnsureNameAvailableAsync(request.Name, id, ct);

        // Sahiblik yaratma anında qeyd olunur — redaktə onu dəyişmir.
        category.Name = request.Name;
        category.IsShared = request.IsShared;
        await db.SaveChangesAsync(ct);

        return ToResponse(category);
    }

    public async Task DeleteAsync(long id, long? ownerId = null, CancellationToken ct = default)
    {
        var category = await db.Subjects.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Subject not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, category.UserId, "You can only modify your own subject");

        category.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureNameAvailableAsync(string name, long? excludeId, CancellationToken ct)
    {
        // Normal sorğu tenant və soft-delete filtrlərini tətbiq edir: silinmiş fənn
        // adı yenidən istifadə oluna bilər, başqa tenant-ın adı isə mane olmur.
        var exists = await db.Subjects
            .AnyAsync(x => x.Name == name && (excludeId == null || x.Id != excludeId), ct);

        if (exists)
        {
            throw new ArgumentException("A main category with this name already exists");
        }
    }

    private static SubjectResponse ToResponse(Subject category) =>
        new(category.Id, category.Name, category.UserId, category.IsShared);

    public Task BulkAsync(BulkRequest request, long? ownerId = null, CancellationToken ct = default)
        => db.BulkOwnedAsync<Subject>(request, ownerId, x => x.UserId, "You can only modify your own subjects", ct);

    /// <summary>Dəyişmək üçün yalnız sahibi olduğu qeyd.</summary>
    private static void EnsureOwned(Subject subject, long? ownerId)
    {
        if (ownerId.HasValue && subject.UserId != ownerId)
        {
            throw new ForbiddenException("You can only modify your own subject");
        }
    }
}
