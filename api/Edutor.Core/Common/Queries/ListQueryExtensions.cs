using System.Linq.Expressions;
using Edutor.Core.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Common.Queries;

/// <summary>Siyahı servislərində təkrarlanan filtr/axtarış əməliyyatları.</summary>
public static class ListQueryExtensions
{
    /// <summary>Silinmişlər görünüşü yalnız istənildikdə soft-delete filtrini söndürür.</summary>
    public static IQueryable<T> ApplyTrashFilter<T>(this IQueryable<T> query, int isDeleted)
        where T : BaseEntity
        => isDeleted == 1
            ? query.IgnoreQueryFilters(new[] { "SoftDelete" }).Where(x => x.DeletedAt != null)
            : query;

    /// <summary>Verilən mətn sahəsində hərf registrinə həssas olmayan axtarış.</summary>
    public static IQueryable<T> SearchBy<T>(this IQueryable<T> query, string? search,
        Expression<Func<T, string?>> selector)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;

        var pattern = $"%{search.Trim()}%";
        var body = Expression.Call(
            typeof(NpgsqlDbFunctionsExtensions),
            nameof(NpgsqlDbFunctionsExtensions.ILike),
            Type.EmptyTypes,
            Expression.Constant(EF.Functions),
            selector.Body,
            Expression.Constant(pattern));

        return query.Where(Expression.Lambda<Func<T, bool>>(body, selector.Parameters[0]));
    }
}
