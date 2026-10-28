using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.Subscription.Requests;
using Edutor.Core.Modules.Subscription.Responses;

namespace Edutor.Core.Modules.Subscription.Abstractions;

public interface ISubscriptionService
{
    /// <summary>Cari tenant-ın planı, effektiv limitləri və bu ayki istifadəsi.</summary>
    Task<SubscriptionResponse> GetAsync(long tenantId, CancellationToken ct = default);

    Task<SubscriptionResponse> GetCurrentAsync(CancellationToken ct = default);

    /// <summary>Yazma əməliyyatına icazə varmı (plan bitibsə read-only).</summary>
    Task EnsureWritableAsync(long tenantId, CancellationToken ct = default);

    Task EnsureCanAddStudentsAsync(int count, CancellationToken ct = default);
    Task EnsureCanAddTeachersAsync(int count, CancellationToken ct = default);
    Task EnsureCanAddGroupsAsync(int count, CancellationToken ct = default);
    Task EnsureCanAddSubjectsAsync(int count, CancellationToken ct = default);
    Task EnsureCanAddLessonsAsync(int count, CancellationToken ct = default);
    /// <summary>Planın müvafiq material növü üçün yükləməyə icazə verdiyini və video limitini yoxlayır.</summary>
    Task EnsureCanUploadAsync(Entities.UploadKind kind = Entities.UploadKind.Video, CancellationToken ct = default);

    /// <summary>Uğurlu konversiyadan sonra mənbə dəqiqələrini cari dövrə yazır.</summary>
    Task RecordVideoMinutesAsync(long tenantId, int minutes, CancellationToken ct = default);

    /// <summary>
    /// Yüklənən faylın ölçüsünü cari dövrün depolama istifadəsinə əlavə edir.
    /// Sayğac hər yükləmədə dərhal yenilənir — limit dərhal düzgün işləsin.
    /// </summary>
    Task AddStorageAsync(long tenantId, long bytes, CancellationToken ct = default);

    /// <summary>Silinən faylların ölçüsünü istifadədən çıxır (sıfırdan aşağı düşmür).</summary>
    Task SubtractStorageAsync(long tenantId, long bytes, CancellationToken ct = default);

    /// <summary>Tenant üçün cari dövrədək icazə verilən eyni vaxtlı konversiya sayı.</summary>
    Task<int> ConcurrentConversionsAsync(long tenantId, CancellationToken ct = default);

    // ---- Əlavə paketlər ----
    Task<TenantAddonResponse> AddAddonAsync(long tenantId, AddAddonRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<TenantAddonResponse>> ListAddonsAsync(long tenantId, CancellationToken ct = default);

    // ---- Plan idarəetməsi (superadmin) ----
    Task<IReadOnlyList<PlanResponse>> ListPlansAsync(bool onlyActive = false, CancellationToken ct = default);
    Task<PlanResponse> GetPlanAsync(long id, CancellationToken ct = default);
    Task<PlanResponse> CreatePlanAsync(PlanRequest request, CancellationToken ct = default);
    Task<PlanResponse> UpdatePlanAsync(long id, PlanRequest request, CancellationToken ct = default);
    Task DeletePlanAsync(long id, CancellationToken ct = default);

    Task AssignPlanAsync(long tenantId, AssignPlanRequest request, CancellationToken ct = default);

    /// <summary>Tenant özü plan seçir (qeydiyyatdan sonrakı plan seçimi).</summary>
    Task SelectPlanAsync(long tenantId, long planId, CancellationToken ct = default);

    /// <summary>Tenant hələ plan seçməyib — panel plan seçimi səhifəsinə yönləndirir.</summary>
    Task<bool> NeedsPlanSelectionAsync(long tenantId, CancellationToken ct = default);
}
