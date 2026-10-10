using LanguageLMS.Core.Common.Entities;
using LanguageLMS.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLMS.Core.Common.Sorting;

/// <summary>Drag&amp;drop sıralama: verilən id ardıcıllığına uyğun sort_order yazır.</summary>
public interface ISortable
{
    int SortOrder { get; set; }
}

public class SortRequest
{
    public List<long> Ids { get; set; } = new();
}

public static class SortHelper
{
    public static async Task ReorderAsync<T>(AppDbContext db, SortRequest request, CancellationToken ct = default)
        where T : BaseEntity, ISortable
    {
        if (request.Ids.Count == 0)
        {
            throw new ArgumentException("Ids are required");
        }

        var items = await db.Set<T>().Where(x => request.Ids.Contains(x.Id)).ToListAsync(ct);
        var map = items.ToDictionary(x => x.Id);

        for (var i = 0; i < request.Ids.Count; i++)
        {
            if (map.TryGetValue(request.Ids[i], out var item))
            {
                item.SortOrder = i + 1;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
