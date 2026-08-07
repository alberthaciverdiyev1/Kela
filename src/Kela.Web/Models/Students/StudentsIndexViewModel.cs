using Kela.Web.Infrastructure;

namespace Kela.Web.Models.Students;

public sealed record StudentsIndexViewModel(
    IReadOnlyList<StudentResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    string? Search)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
