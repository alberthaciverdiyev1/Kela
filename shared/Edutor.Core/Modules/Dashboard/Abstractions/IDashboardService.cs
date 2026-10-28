using Edutor.Core.Modules.Dashboard.Responses;

namespace Edutor.Core.Modules.Dashboard.Abstractions;

public interface IDashboardService
{
    /// <summary><paramref name="ownerId"/> verilərsə yalnız həmin müəllimin qrupları/məzmunu sayılır.</summary>
    Task<DashboardResponse> GetAsync(long? ownerId = null, CancellationToken ct = default);
}
