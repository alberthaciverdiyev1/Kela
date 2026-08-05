namespace Kela.Domain.Common;

/// <summary>
/// Tenant'a tabi her aggregate'te bulunur. EF Core bu arayüzü
/// gören tüm entity'lere otomatik global query filter uygular.
/// </summary>
public interface ITenantEntity
{
    int TenantId { get; set; }
}
