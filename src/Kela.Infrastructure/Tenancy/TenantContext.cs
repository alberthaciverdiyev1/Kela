using Kela.Application.Abstractions.Tenancy;

namespace Kela.Infrastructure.Tenancy;

/// <summary>
/// ICurrentTenant'ın scoped implementasyonu.
/// Tenant'ı middleware (Api/Web katmanı) <see cref="SetTenant"/> ile çözer ve set eder.
/// HttpContext bağımlılığı yoktur → Infrastructure HTTP'ten bağımsız kalır.
/// </summary>
internal sealed class TenantContext : ICurrentTenant
{
    private int? _tenantId;

    public int TenantId => _tenantId
        ?? throw new InvalidOperationException("Tenant bağlamı çözümlenemedi.");

    public bool IsResolved => _tenantId.HasValue;

    public void SetTenant(int tenantId)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId), "TenantId pozitif olmalıdır.");
        }

        _tenantId = tenantId;
    }
}
