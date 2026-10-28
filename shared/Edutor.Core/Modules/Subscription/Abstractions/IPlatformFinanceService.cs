using Edutor.Core.Modules.Subscription.Responses;

namespace Edutor.Core.Modules.Subscription.Abstractions;

/// <summary>Superadmin üçün platforma gəliri: tenant-ların abunəlik və əlavə paket ödənişləri.</summary>
public interface IPlatformFinanceService
{
    Task<PlatformFinanceResponse> GetAsync(int months = 12, CancellationToken ct = default);
}
