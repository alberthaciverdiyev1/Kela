using Edutor.Core.Modules.Sidebar.Requests;
using Edutor.Core.Modules.Sidebar.Responses;

namespace Edutor.Core.Modules.Sidebar.Abstractions;

public interface ISidebarService
{
    Task<SidebarSummaryResponse> GetSummaryAsync(long userId, CancellationToken ct = default);

    Task MarkSeenAsync(long userId, string key, CancellationToken ct = default);

    Task<IReadOnlyList<SidebarLabelResponse>> ListLabelsAsync(CancellationToken ct = default);

    Task<SidebarLabelResponse> SetLabelAsync(string key, SidebarLabelRequest request, CancellationToken ct = default);
}
