using Kela.Core.Modules.Dashboard.Responses;

namespace Kela.Core.Modules.Dashboard.Abstractions;

public interface IDashboardService
{
    /// <summary><paramref name="ownerId"/> verilərsə yalnız həmin müəllimin qrupları/məzmunu sayılır.</summary>
    Task<DashboardResponse> GetAsync(long? ownerId = null, CancellationToken ct = default);
}
