namespace Kela.Core.Common.MultiTenancy;

/// <summary>Cari sorğunun tenant-ını müəyyən edir.</summary>
public interface ITenantProvider
{
    /// <summary>Cari tenant ID; <c>null</c> = tenant seçilməyib.</summary>
    long? TenantId { get; }

    /// <summary>
    /// Cross-tenant rejim (yalnız superadmin): bütün tenant-ların məlumatını görür,
    /// tenant filtri tətbiq olunmur.
    /// </summary>
    bool IgnoreTenant { get; }
}
