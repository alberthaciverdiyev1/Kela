using Kela.Domain.Common;
using Kela.Domain.Tenants.Enums;

namespace Kela.Domain.Tenants;

/// <summary>
/// Multi-tenant kök aggregate. Tenant'a tabi DEĞİLDİR — kendisi tenant'ın ta kendisidir.
/// Slug, subdomain/header çözümlemesinde kullanılır.
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantStatus Status { get; set; } = TenantStatus.Active;
}
