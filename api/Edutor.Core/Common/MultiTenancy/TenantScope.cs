namespace Edutor.Core.Common.MultiTenancy;

/// <summary>
/// Sorğu daxilində tenant-ı açıq şəkildə təyin etmək üçün (məsələn login zamanı,
/// hələ JWT/cookie claim olmadıqda). Scoped-dır.
/// </summary>
public interface ITenantScope
{
    long? Override { get; set; }
}

public sealed class TenantScope : ITenantScope
{
    public long? Override { get; set; }
}
