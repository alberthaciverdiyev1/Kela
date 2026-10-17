namespace Kela.Core.Common.Storage;

/// <summary>
/// Bucket-da tenant qovluğunu müəyyən edir. Bütün modullar faylları
/// <c>{tenant}/&lt;category&gt;</c> altına yazır ki, bir tenant-ın faylları digərindən ayrılsın.
/// </summary>
public interface ITenantFolder
{
    /// <summary>
    /// Tenant qovluğunun adı (slug). Sorğuda tenant yoxdursa (məs. superadmin
    /// tenant-a keçməyib) <paramref name="fallbackTenantId"/> istifadə olunur.
    /// </summary>
    Task<string> ResolveAsync(long? fallbackTenantId = null, CancellationToken ct = default);
}
