using Kela.Web.Helpers;

namespace Kela.Web.Models.Workspaces;

public sealed record WorkspacesIndexViewModel(
    IReadOnlyList<WorkspaceResponse> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
