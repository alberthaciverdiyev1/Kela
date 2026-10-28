using Edutor.Core.Common.Exceptions;
using Edutor.Core.Data;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Subscription.Entities;
using Edutor.Core.Modules.Subscription.Requests;
using Edutor.Core.Modules.Subscription.Responses;
using Edutor.Core.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Subscription.Services;

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
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1080, true, true, true, [], []));
        }

        // Superadmin tenant-ı dəyişəndə qlobal filtr söndürülür — sayım hər halda
        // yalnız seçilmiş tenant üzrə aparılmalıdır.
        var students = await db.Students.CountAsync(x => x.TenantId == tenantId, ct);
        var teachers = await db.Users.CountAsync(x => x.TenantId == tenantId && x.Type == UserTypes.Teacher, ct);
        var groups = await db.Groups.CountAsync(x => x.TenantId == tenantId, ct);
        var subjects = await db.Subjects.CountAsync(x => x.TenantId == tenantId, ct);
        var lessons = await db.Lessons.CountAsync(x => x.TenantId == tenantId, ct);

        var maxStudents = plan.MaxStudents + usage.AddonStudents;
        var maxGroups = plan.MaxGroups + usage.AddonGroups;
        var maxVideo = plan.MonthlyVideoMinutes + usage.AddonVideoMinutes;

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
                maxGroups,
                subjects,
                plan.MaxSubjects,
                lessons,
                plan.MaxLessons,
                usage.VideoMinutes,
                maxVideo,
                usage.StorageBytes,
                plan.StorageBytes,
                plan.MaxVideoHeight,
                plan.AllowVideoUpload,
                plan.AllowAudioUpload,
                plan.AllowFileUpload,
                ParseFeatures(plan.Features),
                Offers(plan)));
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

    // Müəllim, fənn və dərs limitsizdir — yalnız planın bitmə vəziyyəti yoxlanılır.
    public Task EnsureCanAddTeachersAsync(int count, CancellationToken ct = default)
        => EnsureOpenAsync(ct);

    public Task EnsureCanAddGroupsAsync(int count, CancellationToken ct = default)
        => EnsureCapacityAsync(Rule.Groups, count, ct);

    public Task EnsureCanAddSubjectsAsync(int count, CancellationToken ct = default)
        => EnsureOpenAsync(ct);

    public Task EnsureCanAddLessonsAsync(int count, CancellationToken ct = default)
        => EnsureOpenAsync(ct);

    /// <summary>
    /// Yükləmədən əvvəl yoxlanılır: plan həmin material növünə icazə verirmi və video
    /// limiti dolubmu. Video müddəti konversiyadan sonra bilindiyi üçün burada yalnız
    /// "artıq limit dolubmu" yoxlanılır; cari yükləmə bloklanmır.
    /// </summary>
    public async Task EnsureCanUploadAsync(UploadKind kind = UploadKind.Video, CancellationToken ct = default)
    {
        var tenantId = db.CurrentTenantId;
        if (tenantId is not long id) return;

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        var (plan, usage, _, _) = await LoadAsync(id, tenant, ct);

        if (plan is null) return;

        await EnsureWritableAsync(id, ct);
        EnsureKindAllowed(plan, kind);

        var videoLimit = plan.MonthlyVideoMinutes + usage.AddonVideoMinutes;
        if (kind == UploadKind.Video && videoLimit > 0 && usage.VideoMinutes >= videoLimit)
        {
            throw new LimitExceededException(
                AddonKinds.Video,
                $"Aylıq video limiti dolub ({usage.VideoMinutes / 60}/{videoLimit / 60} saat). " +
                "Əlavə video saatı alın və ya planı yüksəldin.",
                usage.VideoMinutes, videoLimit);
        }

    }

    private static void EnsureKindAllowed(SubscriptionPlan plan, UploadKind kind)
    {
        var (allowed, label) = kind switch
        {
            UploadKind.Video => (plan.AllowVideoUpload, "video"),
            UploadKind.Audio => (plan.AllowAudioUpload, "səs"),
            _ => (plan.AllowFileUpload, "fayl (PDF)")
        };

        if (allowed) return;

        throw new ForbiddenException(
            $"{plan.Name} planında {label} yükləməsinə icazə verilmir. " +
            "Yükləmə üçün planı yüksəldin.");
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

        var quantity = Math.Clamp(request.Quantity, 1, 10000);

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenantId, ct)
            ?? throw new KeyNotFoundException($"Tenant not found: {tenantId}");

        var (plan, usage, _, _) = await LoadAsync(tenantId, tenant, ct);

        if (plan is null) throw new InvalidOperationException("Plan tapılmadı");

        var addon = new TenantAddon
        {
            TenantId = tenantId,
            Period = Period(),
            Kind = request.Kind,
            Quantity = quantity,
            Currency = plan.Currency,
            Note = Truncate(request.Note, 190)
        };

        switch (request.Kind)
        {
            case AddonKinds.Students:
                if (plan.PricePerStudent <= 0) throw new ArgumentException("Bu plan üçün əlavə tələbə yeri satılmır");
                usage.AddonStudents += quantity;
                addon.Amount = request.Amount ?? quantity * plan.PricePerStudent;
                break;

            case AddonKinds.Groups:
                if (plan.PricePerGroup <= 0) throw new ArgumentException("Bu plan üçün əlavə qrup satılmır");
                usage.AddonGroups += quantity;
                addon.Amount = request.Amount ?? quantity * plan.PricePerGroup;
                break;

            default:
                if (plan.PricePerVideoHour <= 0) throw new ArgumentException("Bu plan üçün əlavə video saatı satılmır");
                usage.AddonVideoMinutes += quantity * 60;
                addon.Amount = request.Amount ?? quantity * plan.PricePerVideoHour;
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
            .Select(x => new TenantAddonResponse(x.Id, x.Period, x.Kind, x.Quantity,
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
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId && x.DeletedAt == null, ct)
            ?? throw new KeyNotFoundException($"Tenant not found: {tenantId}");

        var plan = await db.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == request.PlanId, ct)
            ?? throw new ArgumentException($"Plan not found: {request.PlanId}");

        var now = DateTimeOffset.UtcNow;

        tenant.PlanId = plan.Id;
        tenant.PlanStartedAt = now;
        tenant.PlanExpiresAt = request.ExpiresAt ?? TrialEnd(plan, now);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Tenant özü plan seçir (qeydiyyatdan sonra). Pulsuz plan dərhal açılır;
    /// ödənişli planlar planın sınaq müddəti qədər pulsuz işləyir.
    /// </summary>
    public async Task SelectPlanAsync(long tenantId, long planId, CancellationToken ct = default)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId && x.DeletedAt == null, ct)
            ?? throw new KeyNotFoundException($"Tenant not found: {tenantId}");

        var plan = await db.SubscriptionPlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == planId, ct)
            ?? throw new ArgumentException($"Plan not found: {planId}");

        if (!plan.IsActive) throw new ArgumentException("Bu plan hazırda əlçatan deyil");

        var now = DateTimeOffset.UtcNow;

        tenant.PlanId = plan.Id;
        tenant.PlanStartedAt = now;
        tenant.PlanExpiresAt = TrialEnd(plan, now);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Pulsuz plan müddətsizdir; ödənişli plan sınaq günü qədər açılır.</summary>
    private static DateTimeOffset? TrialEnd(SubscriptionPlan plan, DateTimeOffset now)
        => plan.MonthlyPrice > 0 && plan.TrialDays > 0 ? now.AddDays(plan.TrialDays) : null;

    public async Task<bool> NeedsPlanSelectionAsync(long tenantId, CancellationToken ct = default)
        => await db.Tenants.AsNoTracking()
            .AnyAsync(x => x.Id == tenantId && x.DeletedAt == null && x.PlanId == null, ct);

    // ---- Daxili ----

    private enum Rule { Students, Groups }

    private async Task EnsureOpenAsync(CancellationToken ct)
    {
        if (db.CurrentTenantId is long id) await EnsureWritableAsync(id, ct);
    }

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
            Rule.Students => (await db.Students.CountAsync(ct), plan.MaxStudents + usage.AddonStudents, "tələbə", AddonKinds.Students),
            _ => (await db.Groups.CountAsync(ct), plan.MaxGroups + usage.AddonGroups, "qrup", AddonKinds.Groups)
        };

        if (limit <= 0) return; // 0 = limitsiz
        if (used + count <= limit) return;

        var title = char.ToUpperInvariant(label[0]) + label[1..];

        throw new LimitExceededException(kind,
            $"{title} limitinə çatmısınız ({used}/{limit}). " +
            "Əlavə limit alın və ya planı yüksəldin.",
            used, limit);
    }

    private async Task<(SubscriptionPlan? Plan, TenantUsage Usage, string Status, int? DaysLeft)> LoadAsync(
        long tenantId, Core.Modules.Tenant.Entities.Tenant? tenant, CancellationToken ct)
    {
        var plan = tenant?.PlanId is long planId
            ? await db.SubscriptionPlans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == planId, ct)
            : null;

        // Plan təyin edilməyibsə ən aşağı ödənişli plan tətbiq olunur — pulsuz plan
        // yalnız istifadəçi onu özü seçəndə qüvvəyə minir.
        if (plan is null)
        {
            plan = await db.SubscriptionPlans.AsNoTracking()
                .Where(x => x.IsActive && x.MonthlyPrice > 0)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
                .FirstOrDefaultAsync(ct)
                ?? await db.SubscriptionPlans.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
                    .FirstOrDefaultAsync(ct);
        }

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

    private static IReadOnlyList<AddonOfferResponse> Offers(SubscriptionPlan plan)
    {
        var offers = new List<AddonOfferResponse>();

        if (plan.PricePerStudent > 0)
        {
            offers.Add(new AddonOfferResponse(AddonKinds.Students, "+ tələbə yeri", plan.PricePerStudent, "tələbə"));
        }

        if (plan.PricePerGroup > 0)
        {
            offers.Add(new AddonOfferResponse(AddonKinds.Groups, "+ qrup", plan.PricePerGroup, "qrup"));
        }

        if (plan.PricePerVideoHour > 0)
        {
            offers.Add(new AddonOfferResponse(AddonKinds.Video, "+ video saatı", plan.PricePerVideoHour, "saat"));
        }

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
        plan.TrialDays = Math.Max(0, request.TrialDays);
        plan.MaxTeachers = request.MaxTeachers;
        plan.MaxStudents = request.MaxStudents;
        plan.MaxGroups = request.MaxGroups;
        plan.MaxSubjects = request.MaxSubjects;
        plan.MaxLessons = request.MaxLessons;
        plan.MonthlyVideoMinutes = request.MonthlyVideoMinutes;
        plan.StorageBytes = Math.Max(0, request.StorageGb) * 1024L * 1024 * 1024;
        plan.ConcurrentConversions = request.ConcurrentConversions;
        plan.MaxVideoHeight = request.MaxVideoHeight is 720 or 1080 ? request.MaxVideoHeight : 1080;
        plan.PricePerStudent = Math.Max(0, request.PricePerStudent);
        plan.PricePerGroup = Math.Max(0, request.PricePerGroup);
        plan.PricePerVideoHour = Math.Max(0, request.PricePerVideoHour);
        plan.OriginalRetentionDays = request.OriginalRetentionDays;
        plan.AllowVideoUpload = request.AllowVideoUpload;
        plan.AllowAudioUpload = request.AllowAudioUpload;
        plan.AllowFileUpload = request.AllowFileUpload;
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
        x.Id, x.Code, x.Name, x.Description, x.MonthlyPrice, x.Currency, x.YearlyFreeMonths, x.TrialDays,
        x.MaxTeachers, x.MaxStudents, x.MaxGroups, x.MaxSubjects, x.MaxLessons,
        x.MonthlyVideoMinutes, x.StorageBytes, x.ConcurrentConversions, x.MaxVideoHeight,
        x.PricePerStudent, x.PricePerGroup, x.PricePerVideoHour, x.OriginalRetentionDays,
        x.AllowVideoUpload, x.AllowAudioUpload, x.AllowFileUpload,
        x.Features, x.IsActive, x.SortOrder);

    private static TenantAddonResponse ToAddonResponse(TenantAddon x)
        => new(x.Id, x.Period, x.Kind, x.Quantity, x.Amount, x.Currency, x.Note, x.CreatedAt);

    private static string? Truncate(string? value, int max)
        => string.IsNullOrWhiteSpace(value) ? null : value.Length <= max ? value : value[..max];
}
