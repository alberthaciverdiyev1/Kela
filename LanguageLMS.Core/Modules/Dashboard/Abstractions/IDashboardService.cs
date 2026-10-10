using LanguageLMS.Core.Modules.Dashboard.Responses;

namespace LanguageLMS.Core.Modules.Dashboard.Abstractions;

public interface IDashboardService
{
    Task<DashboardResponse> GetAsync(CancellationToken ct = default);
}
