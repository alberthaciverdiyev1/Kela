using Kela.Core.Modules.Dashboard.Responses;

namespace Kela.Core.Modules.Dashboard.Abstractions;

public interface IDashboardService
{
    Task<DashboardResponse> GetAsync(CancellationToken ct = default);
}
