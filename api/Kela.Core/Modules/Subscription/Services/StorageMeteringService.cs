using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Media.Entities;
using Kela.Core.Modules.Subscription.Abstractions;
using Kela.Core.Modules.Subscription.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kela.Core.Modules.Subscription.Services;

/// <summary>
/// Hər tenant üçün bucket-ı listələyir, tutulan yeri hesablayır və cari ayın
/// istifadə sətrinə yazır. Planın "orijinal saxlama" müddəti bitmiş faylları
/// yalnız konfiqurasiyada icazə verilibsə silir.
/// </summary>
public sealed class StorageMeteringService(
    AppDbContext db,
    IFileStorage storage,
    ITenantFolder folders,
    IOptions<SubscriptionOptions> options,
    ILogger<StorageMeteringService> logger) : IStorageMeteringService
{
    private SubscriptionOptions Settings => options.Value;

    public async Task<MeteringResult> MeterAsync(CancellationToken ct = default)
    {
        var period = DateTimeOffset.UtcNow.ToString("yyyy-MM");

        var tenants = await db.Tenants.AsNoTracking()
            .Select(x => new { x.Id, x.PlanId })
            .ToListAsync(ct);

        var plans = await db.SubscriptionPlans.AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.OriginalRetentionDays, ct);

        var defaultRetention = await db.SubscriptionPlans.AsNoTracking()
            .Where(x => x.IsActive).OrderBy(x => x.SortOrder)
            .Select(x => x.OriginalRetentionDays)
            .FirstOrDefaultAsync(ct);

        long total = 0;
        var deleted = 0;

        foreach (var tenant in tenants)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var folder = await folders.ResolveAsync(tenant.Id, ct);
                var objects = await storage.ListObjectsAsync($"{folder}/", ct);
                var size = objects.Sum(x => x.SizeBytes);

                total += size;

                var usage = await db.TenantUsages.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Period == period && x.DeletedAt == null, ct);

                if (usage is null)
                {
                    usage = new TenantUsage { TenantId = tenant.Id, Period = period };
                    db.TenantUsages.Add(usage);
                }

                usage.StorageBytes = size;

                if (Settings.DeleteExpiredOriginals)
                {
                    var retention = tenant.PlanId is long id && plans.TryGetValue(id, out var days)
                        ? days
                        : defaultRetention;

                    deleted += await DeleteExpiredOriginalsAsync(tenant.Id, folder, retention, ct);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Tenant {TenantId} üçün storage ölçməsi alınmadı", tenant.Id);
            }
        }

        // Planı 7 gündən çox əvvəl bitmiş tenant-lar deaktiv işarələnir
        // ("read-only" dövrü başa çatıb). Məlumat silinmir.
        var graceCutoff = DateTimeOffset.UtcNow.AddDays(-7);

        var expired = await db.Tenants
            .Where(t => t.IsActive && t.PlanExpiresAt != null && t.PlanExpiresAt < graceCutoff)
            .ToListAsync(ct);

        foreach (var tenant in expired)
        {
            tenant.IsActive = false;
            logger.LogWarning("Tenant {TenantId} abunəliyi bitdiyi üçün deaktiv edildi", tenant.Id);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Storage ölçməsi: {Tenants} tenant, {Gb} GB, {Deleted} orijinal silindi",
            tenants.Count, total / 1024 / 1024 / 1024, deleted);

        return new MeteringResult(tenants.Count, total, deleted);
    }

    /// <summary>
    /// Yalnız yayıma çıxmış videoların orijinalları silinir — konversiya bitməyibsə
    /// mənbə hələ lazımdır. Silinmiş orijinaldan yenidən konversiya mümkün deyil.
    /// </summary>
    private async Task<int> DeleteExpiredOriginalsAsync(long tenantId, string folder, int retentionDays, CancellationToken ct)
    {
        if (retentionDays <= 0) return 0;

        var cutoff = DateTimeOffset.UtcNow.AddDays(-retentionDays);

        var videos = await db.Videos.IgnoreQueryFilters()
            .Where(v => v.TenantId == tenantId && v.DeletedAt == null)
            .Where(v => v.ConversionStatus == VideoStatus.Ready && v.PlaybackUrl != null)
            .Where(v => v.CreatedAt < cutoff)
            .Where(v => v.Path.StartsWith($"{folder}/videos/raw/"))
            .ToListAsync(ct);

        var deleted = 0;

        foreach (var video in videos)
        {
            if (!await storage.ExistsAsync(video.Path, ct)) continue;

            await storage.DeleteAsync(video.Path, ct);
            logger.LogInformation("Köhnə orijinal silindi: video {VideoId} ({Path})", video.Id, video.Path);
            deleted++;
        }

        return deleted;
    }
}
