using Kela.Core.Common.Exceptions;
using Kela.Core.Data;
using Kela.Core.Modules.Subscription.Abstractions;
using Kela.Core.Modules.Subscription.Entities;
using Kela.Core.Modules.Subscription.Requests;
using Kela.Core.Modules.Subscription.Responses;
using Kela.Core.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Subscription.Services;

public sealed class SubscriptionService(AppDbContext db) : ISubscriptionService
{
    /// <summary>Plan bitəndən sonra tam bağlanana qədər yalnız oxuma rejimi.</summary>
    private static readonly TimeSpan ReadOnlyGrace = TimeSpan.FromDays(7);

    private static readonly string[] FeatureKeys = ["certificates", "payments", "crm", "chat", "branding", "api"];

    public async Task<SubscriptionResponse> GetAsync(long tenantId, CancellationToken ct = default)
    {
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenantId, ct);
        var (plan, usage, status, daysLeft) = await LoadAsync(tenantId, tenant, ct);

        if (plan is null)
        {
            return new SubscriptionResponse(null, new SubscriptionUsageResponse(
                null, "none", "Plan təyin edilməyib", "active", null, null,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1080, [], []));
        }

        var students = await db.Students.CountAsync(ct);
        var teachers = await db.Users.CountAsync(x => x.Type == UserTypes.Teacher, ct);
        var groups = await db.Groups.CountAsync(ct);
        var subjects = await db.Subjects.CountAsync(ct);
        var lessons = await db.Lessons.CountAsync(ct);

        var maxStudents = plan.MaxStudents + usage.AddonStudents;
        var maxVideo = plan.MonthlyVideoMinutes + usage.AddonVideoMinutes;
        var maxStorage = plan.StorageBytes + usage.AddonStorageBytes;

        return new SubscriptionResponse(
            new SubscriptionPlanInfo(plan.Id, plan.Code, plan.Name, plan.MonthlyPrice, plan.Currency, plan.YearlyFreeMonths),
            new SubscriptionUsageResponse(
                plan.Id,
                plan.Code,
                plan.Name,
                status,
                daysLeft,
                tenant?.PlanExpiresAt,
                students,
                maxStudents,
                teachers,
                plan.MaxTeachers,
                groups,
                plan.MaxGroups,
                subjects,
                plan.MaxSubjects,
                lessons,
                plan.MaxLessons,
                usage.VideoMinutes,
                maxVideo,
                usage.StorageBytes,
                maxStorage,
                plan.MaxVideoHeight,
                ParseFeatures(plan.Features),
                Offers(plan, usage, maxStudents, maxVideo, maxStorage)));
    }

    public async Task<SubscriptionResponse> GetCurrentAsync(CancellationToken ct = default)
    {
        var tenantId = db.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant context is required");

        return await GetAsync(tenantId, ct);
    }

    public async Task EnsureWritableAsync(long tenantId, CancellationToken ct = default)
    {
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenantId, ct);
        var (_, _, status, _) = await LoadAsync(tenantId, tenant, ct);

        if (status == "suspended")
        {
            throw new ForbiddenException("Abunəlik bitib. Davam etmək üçün planı yeniləyin.");
        }

        if (status == "readonly")
        {
            throw new ForbiddenException("Abunəlik müddəti bitib — hazırda yalnız oxuma rejimi. Planı yeniləyin.");
        }
    }

    public Task EnsureCanAddStudentsAsync(int count, CancellationToken ct = default)
        => EnsureCapacityAsync(Rule.Students, count, ct);

    public Task EnsureCanAddTeachersAsync(int count, CancellationToken ct = default)
        => EnsureCapacityAsync(Rule.Teachers, count, ct);

    public Task EnsureCanAddGroupsAsync(int count, CancellationToken ct = default)
        => EnsureCapacityAsync(Rule.Groups, count, ct);

    public Task EnsureCanAddSubjectsAsync(int count, CancellationToken ct = default)
        => EnsureCapacityAsync(Rule.Subjects, count, ct);

    public Task EnsureCanAddLessonsAsync(int count, CancellationToken ct = default)
        => EnsureCapacityAsync(Rule.Lessons, count, ct);

    /// <summary>
    /// Video yükləməzdən əvvəl yoxlanılır. Müddət konversiyadan sonra bilindiyi üçün
    /// burada yalnız "artıq limit dolubmu" yoxlanılır; cari yükləmə bloklanmır.
    /// </summary>
    public async Task EnsureCanUploadAsync(bool isVideo = true, CancellationToken ct = default)
    {
        var tenantId = db.CurrentTenantId;
        if (tenantId is not long id) return;

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        var (plan, usage, _, _) = await LoadAsync(id, tenant, ct);

        if (plan is null) return;

        await EnsureWritableAsync(id, ct);

        var videoLimit = plan.MonthlyVideoMinutes + usage.AddonVideoMinutes;
        if (isVideo && videoLimit > 0 && usage.VideoMinutes >= videoLimit)
        {
            throw new LimitExceededException(
                AddonKinds.Video,
                $"Aylıq video limiti dolub ({usage.VideoMinutes}/{videoLimit} dəqiqə). " +
                "Əlavə paket alın və ya planı yüksəldin.",
                usage.VideoMinutes, videoLimit);
        }

        var storageLimit = plan.StorageBytes + usage.AddonStorageBytes;
        if (storageLimit > 0 && usage.StorageBytes >= storageLimit)
        {
            throw new LimitExceededException(
                AddonKinds.Storage,
                $"Depolama limiti dolub ({usage.StorageBytes / 1024 / 1024} MB / {storageLimit / 1024 / 1024} MB). " +
                "Köhnə faylları silin, əlavə paket alın və ya planı yüksəldin.",
                usage.StorageBytes, storageLimit);
        }
    }

    public async Task RecordVideoMinutesAsync(long tenantId, int minutes, CancellationToken ct = default)
    {
        if (minutes <= 0) return;

        var usage = await GetOrCreateUsageAsync(tenantId, Period(), ct);
        usage.VideoMinutes += minutes;
        await db.SaveChangesAsync(ct);
    }

    public async Task AddStorageAsync(long tenantId, long bytes, CancellationToken ct = default)
    {
        if (bytes <= 0) return;

        var usage = await GetOrCreateUsageAsync(tenantId, Period(), ct);
        usage.StorageBytes += bytes;
        await db.SaveChangesAsync(ct);
    }

    public async Task SubtractStorageAsync(long tenantId, long bytes, CancellationToken ct = default)
    {
        if (bytes <= 0) return;

        var usage = await GetOrCreateUsageAsync(tenantId, Period(), ct);
        usage.StorageBytes = Math.Max(0, usage.StorageBytes - bytes);
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> ConcurrentConversionsAsync(long tenantId, CancellationToken ct = default)
    {
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenantId, ct);
        var (plan, _, _, _) = await LoadAsync(tenantId, tenant, ct);

        if (plan is null) return int.MaxValue;

        return plan.ConcurrentConversions <= 0 ? int.MaxValue : plan.ConcurrentConversions;
    }

    // ---- Əlavə paketlər ----

    public async Task<TenantAddonResponse> AddAddonAsync(long tenantId, AddAddonRequest request, CancellationToken ct = default)
    {
        if (!AddonKinds.All.Contains(request.Kind))
        {
            throw new ArgumentException($"kind must be one of: {string.Join(", ", AddonKinds.All)}");
        }

        var packs = Math.Clamp(request.Packs, 1, 100);

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenantId, ct)
            ?? throw new KeyNotFoundException($"Tenant not found: {tenantId}");

        var (plan, usage, _, _) = await LoadAsync(tenantId, tenant, ct);

        if (plan is null) throw new InvalidOperationException("Plan tapılmadı");

        var addon = new TenantAddon
        {
            TenantId = tenantId,
            Period = Period(),
            Kind = request.Kind,
            Packs = packs,
            Currency = plan.Currency,
            Note = Truncate(request.Note, 190)
        };

        switch (request.Kind)
        {
            case AddonKinds.Students:
                var students = packs * plan.AddonStudentPackSize;
                if (ExceedsCap(plan.MaxStudents, usage.AddonStudents + students, plan.MaxAddonPercent))
                {
                    throw new ArgumentException("Bu plan üçün maksimum əlavə tələbə sayına çatmısınız");
                }

                usage.AddonStudents += students;
                addon.Quantity = students;
                addon.Amount = request.Amount ?? packs * plan.AddonStudentPackPrice;
                break;

            case AddonKinds.Video:
                var minutes = packs * plan.AddonVideoPackMinutes;
                if (ExceedsCap(plan.MonthlyVideoMinutes, usage.AddonVideoMinutes + minutes, plan.MaxAddonPercent))
                {
                    throw new ArgumentException("Bu plan üçün maksimum əlavə video limitinə çatmısınız");
                }

                usage.AddonVideoMinutes += minutes;
                addon.Quantity = minutes;
                addon.Amount = request.Amount ?? packs * plan.AddonVideoPackPrice;
                break;

            default:
                var bytes = packs * plan.AddonStoragePackBytes;
                if (ExceedsCap(plan.StorageBytes, usage.AddonStorageBytes + bytes, plan.MaxAddonPercent))
                {
                    throw new ArgumentException("Bu plan üçün maksimum əlavə yer limitinə çatmısınız");
                }

                usage.AddonStorageBytes += bytes;
                addon.Bytes = bytes;
                addon.Amount = request.Amount ?? packs * plan.AddonStoragePackPrice;
                break;
        }

        db.TenantAddons.Add(addon);
        await db.SaveChangesAsync(ct);

        return ToAddonResponse(addon);
    }

    public async Task<IReadOnlyList<TenantAddonResponse>> ListAddonsAsync(long tenantId, CancellationToken ct = default)
        => await db.TenantAddons.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.Id)
            .Take(100)
            .Select(x => new TenantAddonResponse(x.Id, x.Period, x.Kind, x.Packs, x.Quantity, x.Bytes,
                x.Amount, x.Currency, x.Note, x.CreatedAt))
            .ToListAsync(ct);

    // ---- Plan idarəetməsi ----

    public async Task<IReadOnlyList<PlanResponse>> ListPlansAsync(bool onlyActive = false, CancellationToken ct = default)
    {
        var query = db.SubscriptionPlans.AsNoTracking();
        if (onlyActive) query = query.Where(x => x.IsActive);

        return await query.OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => Map(x))
            .ToListAsync(ct);
    }

    public async Task<PlanResponse> GetPlanAsync(long id, CancellationToken ct = default)
    {
        var plan = await db.SubscriptionPlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Plan not found: {id}");

        return Map(plan);
    }

    public async Task<PlanResponse> CreatePlanAsync(PlanRequest request, CancellationToken ct = default)
    {
        var plan = new SubscriptionPlan();
        Apply(plan, request);
        plan.CreatedAt = DateTimeOffset.UtcNow;

        db.SubscriptionPlans.Add(plan);
        await db.SaveChangesAsync(ct);

        return Map(plan);
    }

    public async Task<PlanResponse> UpdatePlanAsync(long id, PlanRequest request, CancellationToken ct = default)
    {
        var plan = await db.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Plan not found: {id}");

        Apply(plan, request);
        plan.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Map(plan);
    }

    public async Task DeletePlanAsync(long id, CancellationToken ct = default)
    {
        var plan = await db.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Plan not found: {id}");

        var used = await db.Tenants.AnyAsync(x => x.PlanId == id, ct);
        if (used) throw new ArgumentException("Bu plan tenant-lara təyin edilib — əvvəlcə onları başqa plana keçirin");

        db.SubscriptionPlans.Remove(plan);
        await db.SaveChangesAsync(ct);
    }

    public async Task AssignPlanAsync(long tenantId, AssignPlanRequest request, CancellationToken ct = default)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId, ct)
            ?? throw new KeyNotFoundException($"Tenant not found: {tenantId}");

        if (!await db.SubscriptionPlans.AnyAsync(x => x.Id == request.PlanId, ct))
        {
            throw new ArgumentException($"Plan not found: {request.PlanId}");
        }

        tenant.PlanId = request.PlanId;
        tenant.PlanStartedAt = DateTimeOffset.UtcNow;
        tenant.PlanExpiresAt = request.ExpiresAt;
        await db.SaveChangesAsync(ct);
    }

    // ---- Daxili ----

    private enum Rule { Students, Teachers, Groups, Subjects, Lessons }

    private async Task EnsureCapacityAsync(Rule rule, int count, CancellationToken ct)
    {
        var tenantId = db.CurrentTenantId;
        if (tenantId is not long id) return;

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        var (plan, usage, _, _) = await LoadAsync(id, tenant, ct);

        if (plan is null) return;

        await EnsureWritableAsync(id, ct);

        var (used, limit, label, kind) = rule switch
        {
            Rule.Students => (await db.Students.CountAsync(ct), plan.MaxStudents + usage.AddonStudents, "tələbə", "students"),
            Rule.Teachers => (await db.Users.CountAsync(x => x.Type == UserTypes.Teacher, ct), plan.MaxTeachers, "müəllim", "teachers"),
            Rule.Groups => (await db.Groups.CountAsync(ct), plan.MaxGroups, "qrup", "groups"),
            Rule.Subjects => (await db.Subjects.CountAsync(ct), plan.MaxSubjects, "fənn", "subjects"),
            _ => (await db.Lessons.CountAsync(ct), plan.MaxLessons, "dərs", "lessons")
        };

        if (limit <= 0) return; // 0 = limitsiz
        if (used + count <= limit) return;

        throw new LimitExceededException(kind,
            $"{label} limitinə çatmısınız ({used}/{limit}). " +
            "Əlavə paket alın və ya planı yüksəldin.",
            used, limit);
    }

    private async Task<(SubscriptionPlan? Plan, TenantUsage Usage, string Status, int? DaysLeft)> LoadAsync(
        long tenantId, Core.Modules.Tenant.Entities.Tenant? tenant, CancellationToken ct)
    {
        var plan = tenant?.PlanId is long planId
            ? await db.SubscriptionPlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == planId, ct)
            : null;

        // Plan təyin edilməyibsə ən aşağı (default) plan tətbiq olunur.
        plan ??= await db.SubscriptionPlans.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .FirstOrDefaultAsync(ct);

        var usage = await GetOrCreateUsageAsync(tenantId, Period(), ct);

        var (status, daysLeft) = Status(tenant);
        return (plan, usage, status, daysLeft);
    }

    private static (string Status, int? DaysLeft) Status(Core.Modules.Tenant.Entities.Tenant? tenant)
    {
        if (tenant is null || !tenant.IsActive) return ("suspended", null);
        if (tenant.PlanExpiresAt is not { } expires) return ("active", null);

        var now = DateTimeOffset.UtcNow;

        if (now <= expires) return ("active", (int)Math.Ceiling((expires - now).TotalDays));
        if (now <= expires + ReadOnlyGrace) return ("readonly", 0);

        return ("suspended", 0);
    }

    private async Task<TenantUsage> GetOrCreateUsageAsync(long tenantId, string period, CancellationToken ct)
    {
        var usage = await db.TenantUsages.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Period == period && x.DeletedAt == null, ct);

        if (usage is not null) return usage;

        usage = new TenantUsage { TenantId = tenantId, Period = period };
        db.TenantUsages.Add(usage);
        await db.SaveChangesAsync(ct);

        return usage;
    }

    private static string Period() => DateTimeOffset.UtcNow.ToString("yyyy-MM");

    private static bool ExceedsCap(int planLimit, int addonTotal, int maxPercent)
        => ExceedsCap((long)planLimit, addonTotal, maxPercent);

    private static bool ExceedsCap(long planLimit, long addonTotal, int maxPercent)
    {
        if (planLimit <= 0) return false;
        if (maxPercent <= 0) return true;

        return addonTotal > planLimit * maxPercent / 100;
    }

    private static IReadOnlyList<string> ParseFeatures(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(json);
            return [.. FeatureKeys.Where(key =>
                document.RootElement.TryGetProperty(key, out var value) && value.ValueKind == System.Text.Json.JsonValueKind.True)];
        }
        catch (System.Text.Json.JsonException)
        {
            return [];
        }
    }

    private static IReadOnlyList<AddonOfferResponse> Offers(
        SubscriptionPlan plan, TenantUsage usage, int maxStudents, int maxVideo, long maxStorage)
    {
        var offers = new List<AddonOfferResponse>();

        if (plan.AddonStudentPackPrice > 0 && plan.MaxStudents > 0)
        {
            var room = (plan.MaxStudents * plan.MaxAddonPercent / 100) - usage.AddonStudents;
            if (room >= plan.AddonStudentPackSize)
            {
                offers.Add(new AddonOfferResponse(AddonKinds.Students, "+ tələbə yeri", plan.AddonStudentPackSize,
                    plan.AddonStudentPackPrice, "tələbə", room / plan.AddonStudentPackSize));
            }
        }

        if (plan.AddonVideoPackPrice > 0 && plan.MonthlyVideoMinutes > 0)
        {
            var room = (plan.MonthlyVideoMinutes * plan.MaxAddonPercent / 100) - usage.AddonVideoMinutes;
            if (room >= plan.AddonVideoPackMinutes)
            {
                offers.Add(new AddonOfferResponse(AddonKinds.Video, "+ video vaxtı", plan.AddonVideoPackMinutes,
                    plan.AddonVideoPackPrice, "dəqiqə", room / plan.AddonVideoPackMinutes));
            }
        }

        if (plan.AddonStoragePackPrice > 0 && plan.StorageBytes > 0)
        {
            var room = (plan.StorageBytes * plan.MaxAddonPercent / 100) - usage.AddonStorageBytes;
            if (room >= plan.AddonStoragePackBytes)
            {
                offers.Add(new AddonOfferResponse(AddonKinds.Storage, "+ depolama", (int)(plan.AddonStoragePackBytes / (1024 * 1024 * 1024)),
                    plan.AddonStoragePackPrice, "GB", (int)(room / plan.AddonStoragePackBytes)));
            }
        }

        _ = maxStudents;
        _ = maxVideo;
        _ = maxStorage;

        return offers;
    }

    private static void Apply(SubscriptionPlan plan, PlanRequest request)
    {
        plan.Code = request.Code.Trim().ToLowerInvariant();
        plan.Name = request.Name.Trim();
        plan.Description = request.Description;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "AZN" : request.Currency.Trim().ToUpperInvariant();
        plan.YearlyFreeMonths = request.YearlyFreeMonths;
        plan.MaxTeachers = request.MaxTeachers;
        plan.MaxStudents = request.MaxStudents;
        plan.MaxGroups = request.MaxGroups;
        plan.MaxSubjects = request.MaxSubjects;
        plan.MaxLessons = request.MaxLessons;
        plan.MonthlyVideoMinutes = request.MonthlyVideoMinutes;
        plan.StorageBytes = Math.Max(0, request.StorageGb) * 1024L * 1024 * 1024;
        plan.ConcurrentConversions = request.ConcurrentConversions;
        plan.MaxVideoHeight = request.MaxVideoHeight is 720 or 1080 ? request.MaxVideoHeight : 1080;
        plan.AddonStudentPackSize = Math.Max(1, request.AddonStudentPackSize);
        plan.AddonStudentPackPrice = request.AddonStudentPackPrice;
        plan.AddonVideoPackMinutes = Math.Max(1, request.AddonVideoPackMinutes);
        plan.AddonVideoPackPrice = request.AddonVideoPackPrice;
        plan.AddonStoragePackBytes = Math.Max(1, request.AddonStoragePackGb) * 1024L * 1024 * 1024;
        plan.AddonStoragePackPrice = request.AddonStoragePackPrice;
        plan.MaxAddonPercent = Math.Clamp(request.MaxAddonPercent, 0, 500);
        plan.OriginalRetentionDays = request.OriginalRetentionDays;
        plan.IsActive = request.IsActive;
        plan.SortOrder = request.SortOrder;

        var features = (request.Features ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(FeatureKeys.Contains)
            .Distinct()
            .ToDictionary(key => key, _ => true);

        plan.Features = System.Text.Json.JsonSerializer.Serialize(features);
    }

    private static PlanResponse Map(SubscriptionPlan x) => new(
        x.Id, x.Code, x.Name, x.Description, x.MonthlyPrice, x.Currency, x.YearlyFreeMonths,
        x.MaxTeachers, x.MaxStudents, x.MaxGroups, x.MaxSubjects, x.MaxLessons,
        x.MonthlyVideoMinutes, x.StorageBytes, x.ConcurrentConversions, x.MaxVideoHeight,
        x.AddonStudentPackSize, x.AddonStudentPackPrice, x.AddonVideoPackMinutes, x.AddonVideoPackPrice,
        x.AddonStoragePackBytes, x.AddonStoragePackPrice, x.MaxAddonPercent, x.OriginalRetentionDays,
        x.Features, x.IsActive, x.SortOrder);

    private static TenantAddonResponse ToAddonResponse(TenantAddon x)
        => new(x.Id, x.Period, x.Kind, x.Packs, x.Quantity, x.Bytes, x.Amount, x.Currency, x.Note, x.CreatedAt);

    private static string? Truncate(string? value, int max)
        => string.IsNullOrWhiteSpace(value) ? null : value.Length <= max ? value : value[..max];
}
