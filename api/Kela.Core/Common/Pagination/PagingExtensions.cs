using Kela.Core.Common.Requests;
using Kela.Core.Common.Responses;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Common.Pagination;

/// <summary>Siyahı sorğuları üçün ortaq səhifələmə hesablamaları.</summary>
public static class Paging
{
    public static (int Page, int PerPage) Normalize(
        ListRequestBase request, int defaultPerPage = 20, int maxPerPage = 500)
        => (request.Page < 1 ? 1 : request.Page,
            request.PerPage < 1 ? defaultPerPage : Math.Min(request.PerPage, maxPerPage));

    public static (int Page, int PerPage) Normalize(
        int page, int perPage, int defaultPerPage = 20, int maxPerPage = 500)
        => (page < 1 ? 1 : page,
            perPage < 1 ? defaultPerPage : Math.Min(perPage, maxPerPage));

    public static int LastPage(int total, int perPage)
        => perPage <= 0 ? 1 : (int)Math.Ceiling(total / (double)perPage);
}

public static class PagingExtensions
{
    /// <summary>Sorğunu səhifələyir və ortaq <see cref="PagedResponse{T}"/> yaradır.</summary>
    public static async Task<PagedResponse<T>> ToPagedAsync<T>(
        this IQueryable<T> query,
        ListRequestBase request,
        CancellationToken ct = default,
        int defaultPerPage = 20,
        int maxPerPage = 500)
    {
        var (page, perPage) = Paging.Normalize(request, defaultPerPage, maxPerPage);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync(ct);

        return new PagedResponse<T>(items, page, Paging.LastPage(total, perPage), perPage, total);
    }

    /// <summary>Səhifələyir və hər elementi cavab modelinə çevirir.</summary>
    public static async Task<PagedResponse<TResult>> ToPagedAsync<T, TResult>(
        this IQueryable<T> query,
        ListRequestBase request,
        Func<T, TResult> map,
        CancellationToken ct = default,
        int defaultPerPage = 20,
        int maxPerPage = 500)
    {
        var (page, perPage) = Paging.Normalize(request, defaultPerPage, maxPerPage);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync(ct);
        var data = items.Select(map).ToList();

        return new PagedResponse<TResult>(data, page, Paging.LastPage(total, perPage), perPage, total);
    }
}
