using LanguageLMS.Core.Common.Entities;
using LanguageLMS.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Common.Bulk;

public static class BulkActions
{
    public const string Delete = "delete";
    public const string Restore = "restore";
    public const string Activate = "activate";
    public const string Deactivate = "deactivate";
}

public class BulkRequest
{
    public List<long> Ids { get; set; } = new();

    public string Action { get; set; } = BulkActions.Delete;
}

public static class BulkHelper
{
    public static async Task ApplyAsync<T>(AppDbContext db, BulkRequest request, CancellationToken ct = default)
        where T : BaseEntity
    {
        if (request.Ids.Count == 0)
        {
            throw new ArgumentException("Ids are required");
        }

        var items = await db.Set<T>().IgnoreQueryFilters().Where(x => request.Ids.Contains(x.Id)).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var isActiveProperty = typeof(T).GetProperty("IsActive");

        foreach (var item in items)
        {
            switch (request.Action)
            {
                case BulkActions.Delete:
                    item.DeletedAt = now;
                    break;
                case BulkActions.Restore:
                    item.DeletedAt = null;
                    break;
                case BulkActions.Activate:
                case BulkActions.Deactivate:
                    if (isActiveProperty is null)
                    {
                        throw new ArgumentException($"{typeof(T).Name} does not support activate/deactivate");
                    }
                    isActiveProperty.SetValue(item, request.Action == BulkActions.Activate);
                    break;
                default:
                    throw new ArgumentException($"Unsupported action: {request.Action}");
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
