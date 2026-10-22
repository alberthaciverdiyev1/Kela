using Kela.Core.Common.Responses;
using Kela.Core.Modules.Subscription.Requests;
using Kela.Core.Modules.Subscription.Responses;

namespace Kela.Core.Modules.Subscription.Abstractions;

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
    Task EnsureCanUploadAsync(CancellationToken ct = default);

    /// <summary>Uğurlu konversiyadan sonra mənbə dəqiqələrini cari dövrə yazır.</summary>
    Task RecordVideoMinutesAsync(long tenantId, int minutes, CancellationToken ct = default);

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
}
