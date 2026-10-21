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

    /// <summary>Aktiv/deaktiv statusunu yalnız sahibi (və ya admin) dəyişə bilər.</summary>
    public static async Task SetActiveAsync<T>(this AppDbContext db, long id, bool isActive, long? ownerId,
        Expression<Func<T, long?>> owner, string forbiddenMessage, CancellationToken ct = default)
        where T : BaseEntity, IActivatable
    {
        var entity = await db.Set<T>().IgnoreQueryFilters(new[] { "SoftDelete" })
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"{typeof(T).Name} not found: {id}");

        OwnershipGuard.EnsureOwned(ownerId, owner.Compile()(entity), forbiddenMessage);
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
