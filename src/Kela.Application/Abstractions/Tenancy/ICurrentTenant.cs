namespace Kela.Application.Abstractions.Tenancy;

/// <summary>
/// Şu anki isteğin bağlı olduğu tenant'ı temsil eder.
/// Tenant çözümleme sorumluluğu HTTP katmanındadır (middleware):
/// middleware <see cref="SetTenant"/> ile bağlamı besler, Application/Infrastructure
/// sadece okur. Böylece HTTP bağımlılığı altyapı katmanına sızmaz.
/// </summary>
public interface ICurrentTenant
{
    int TenantId { get; }
    bool IsResolved { get; }

    /// <summary>İsteğin tenant'ını çözen middleware tarafından çağrılır.</summary>
    void SetTenant(int tenantId);
}
