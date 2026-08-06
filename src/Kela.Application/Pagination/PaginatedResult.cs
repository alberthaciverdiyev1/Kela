namespace Kela.Application.Pagination;

public sealed record PaginatedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
