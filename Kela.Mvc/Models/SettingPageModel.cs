using Kela.Core.Modules.Setting.Responses;
using Kela.Core.Modules.Tenant.Responses;

namespace Kela.Mvc.Models;

/// <summary>Kurs profili səhifəsi: tenant (kurs) məlumatları + kurs parametrləri.</summary>
public class SettingPageModel
{
    public SettingResponse Setting { get; set; } = default!;

    public TenantResponse? Tenant { get; set; }

    public bool HasTenant => Tenant is not null;
}
