using System.Linq.Expressions;
using Kela.Core.Common.Entities;
using Kela.Core.Common.Requests;

namespace Kela.Core.Common.Sorting;

public static class ListSort
{
    public const string Created = "created";
    public const string Updated = "updated";
    public const string Name = "name";
    public const string Order = "order";

    public static bool IsAscending(ListRequestBase request) =>
        string.Equals(request.SortDir, "asc", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Siyahı sıralaması. Standart: ən yenilər əvvəldə (created_at desc).
    /// "name" və "order" yalnız müvafiq selektor verildikdə işləyir.
    /// </summary>
    public static IQueryable<T> ApplyListSort<T>(
        this IQueryable<T> query,
        ListRequestBase request,
        Expression<Func<T, string?>>? name = null,
        Expression<Func<T, int>>? order = null) where T : BaseEntity
    {
        var asc = IsAscending(request);

        return request.SortBy switch
        {
            Name when name is not null => asc ? query.OrderBy(name) : query.OrderByDescending(name),
            Order when order is not null => asc ? query.OrderBy(order) : query.OrderByDescending(order),
            Updated => asc ? query.OrderBy(x => x.UpdatedAt) : query.OrderByDescending(x => x.UpdatedAt),
            _ => asc ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
        };
    }
}
