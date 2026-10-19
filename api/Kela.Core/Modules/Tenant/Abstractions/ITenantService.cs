using Kela.Core.Modules.Tenant.Requests;
using Kela.Core.Modules.Tenant.Responses;

namespace Kela.Core.Modules.Tenant.Abstractions;

public interface ITenantService
{
    Task<IReadOnlyList<TenantResponse>> ListAsync(CancellationToken ct = default);

    /// <summary>Login səhifəsi üçün yalnız aktiv tenant-lar (public).</summary>
    Task<IReadOnlyList<TenantResponse>> ListPublicAsync(CancellationToken ct = default);

    Task<TenantResponse?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Cari sorğunun tenant-ı (kurs admini öz kursunu idarə edir).</summary>
    Task<TenantResponse?> GetCurrentAsync(CancellationToken ct = default);

    Task<TenantResponse> UpdateCurrentAsync(UpdateTenantProfileRequest request, CancellationToken ct = default);

    Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);

    /// <summary>Public müraciət: aktiv olmayan tenant + ilk admin hesabı yaradır.</summary>
    Task<TenantResponse> RegisterAsync(RegisterTenantRequest request, CancellationToken ct = default);

    /// <summary>Superadmin təsdiqi — tenant-ı aktivləşdirir/söndürür.</summary>
    Task<TenantResponse> SetActiveAsync(long id, bool isActive, CancellationToken ct = default);

    /// <summary>Gözləyən tenant müraciətlərinin sayı.</summary>
    Task<int> CountPendingAsync(CancellationToken ct = default);

    /// <summary>Gözləyən müraciəti rədd edib tenant-ı silir (yalnız pending/deaktiv).</summary>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
