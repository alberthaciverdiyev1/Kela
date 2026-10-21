using System.Linq.Expressions;
using Kela.Core.Common.Entities;
using Kela.Core.Common.Exceptions;
using Kela.Core.Common.Authorization;
using Kela.Core.Common.Sorting;
using Kela.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Common.Bulk;

/// <summary>Sahiblik yoxlaması ilə toplu əməliyyat və sıralama — CRUD servisləri üçün ortaq.</summary>
public static class OwnedCrudExtensions
{
    public static async Task BulkOwnedAsync<T>(this AppDbContext db, BulkRequest request, long? ownerId,
        Expression<Func<T, long?>> owner, string forbiddenMessage, CancellationToken ct = default)
        where T : BaseEntity
    {
        if (ownerId is long id) await EnsureAllOwnedAsync(db, request.Ids, id, owner, forbiddenMessage, ct);
        await BulkHelper.ApplyAsync<T>(db, request, ct);
    }

    public static async Task ReorderOwnedAsync<T>(this AppDbContext db, SortRequest request, long? ownerId,
        Expression<Func<T, long?>> owner, string forbiddenMessage, CancellationToken ct = default)
        where T : BaseEntity, ISortable
    {
        if (ownerId is long id) await EnsureAllOwnedAsync(db, request.Ids, id, owner, forbiddenMessage, ct);
        await SortHelper.ReorderAsync<T>(db, request, ct);
    }

    /// <summary>
    /// Aktiv/deaktiv statusunu sahibi (və ya admin) dəyişə bilər. <paramref name="sharedForOthers"/>
    /// verilərsə, qeyd paylaşıldıqda digər müəllimlər də statusu dəyişə bilər — paylaşım
    /// "baxmaq" deyil, birlikdə işləmək deməkdir.
    /// </summary>
    public static async Task SetActiveAsync<T>(this AppDbContext db, long id, bool isActive, long? ownerId,
        Expression<Func<T, long?>> owner, string forbiddenMessage, CancellationToken ct = default,
        Expression<Func<T, bool>>? sharedForOthers = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
        where T : BaseEntity, IActivatable
    {
        var query = db.Set<T>().IgnoreQueryFilters(new[] { "SoftDelete" });
        if (include is not null) query = include(query);

        var entity = await query.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"{typeof(T).Name} not found: {id}");

        var shared = sharedForOthers is not null && sharedForOthers.Compile()(entity);

        OwnershipGuard.EnsureReadable(ownerId, owner.Compile()(entity), shared, forbiddenMessage);
        entity.IsActive = isActive;
        await db.SaveChangesAsync(ct);
    }

    private static async Task EnsureAllOwnedAsync<T>(AppDbContext db, IEnumerable<long> ids, long ownerId,
        Expression<Func<T, long?>> owner, string message, CancellationToken ct)
        where T : BaseEntity
    {
        var wanted = ids.Distinct().ToList();
        var owners = await db.Set<T>().IgnoreQueryFilters(new[] { "SoftDelete" })
            .Where(x => wanted.Contains(x.Id))
            .Select(owner)
            .ToListAsync(ct);

        if (owners.Count(x => x == ownerId) != wanted.Count)
        {
            throw new ForbiddenException(message);
        }
    }
}
