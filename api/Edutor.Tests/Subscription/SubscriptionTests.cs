using Edutor.Core.Common.Exceptions;
using Edutor.Core.Common.MultiTenancy;
using Edutor.Core.Data;
using Edutor.Core.Modules.Subscription;
using Edutor.Core.Modules.Subscription.Entities;
using Edutor.Core.Modules.Subscription.Requests;
using Edutor.Core.Modules.Subscription.Services;
using Edutor.Core.Modules.Subscription.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Tests.Subscription;

/// <summary>
/// Abunəlik: plan limitləri, əlavə paketlər və plan bitməsi.
/// Servis tenant kontekstindən istifadə etdiyi üçün ayrıca DbContext qurulur.
/// </summary>
[Collection("api")]
public class SubscriptionTests(ApiFactory factory)
{
    private sealed class FixedTenant(long tenantId) : ITenantProvider
    {
        public long? TenantId => tenantId;

        public bool IgnoreTenant => false;
    }

    private AppDbContext Db(long tenantId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(factory.ConnectionString)
            .Options;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tenancy:Strict"] = "false"
            })
            .Build();

        return new AppDbContext(options, new FixedTenant(tenantId), config);
    }

    private async Task<long> TenantAsync(AppDbContext db, long? planId = null, DateTimeOffset? expires = null)
    {
        var tenant = new Core.Modules.Tenant.Entities.Tenant
        {
            Name = $"T-{Guid.NewGuid():N}",
            Slug = $"t-{Guid.NewGuid():N}",
            IsActive = true,
            PlanId = planId,
            PlanExpiresAt = expires
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        return tenant.Id;
    }

    private static PlanRequest Request(string code, int maxStudents, int monthlyVideoMinutes) => new()
    {
        Code = code,
        Name = code,
        MonthlyPrice = 50,
        Currency = "AZN",
        MaxStudents = maxStudents,
        MonthlyVideoMinutes = monthlyVideoMinutes,
        StorageGb = 10,
        MaxTeachers = 2,
        MaxGroups = 5,
        MaxSubjects = 5,
        MaxLessons = 50,
        ConcurrentConversions = 2,
        MaxVideoHeight = 1080,
        PricePerStudent = 8,
        PricePerGroup = 5,
        PricePerVideoHour = 12,
        OriginalRetentionDays = 30,
        IsActive = true,
        SortOrder = 10
    };

    [Fact]
    public async Task A_plan_can_be_created_and_edited()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var created = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 100, 600));
        var updated = await service.UpdatePlanAsync(created.Id, Request(created.Code, 200, 1200));

        Assert.Equal(200, updated.MaxStudents);
        Assert.Equal(1200, updated.MonthlyVideoMinutes);
        Assert.Equal(10L * 1024 * 1024 * 1024, updated.StorageBytes);
    }

    [Fact]
    public async Task A_deleted_student_frees_a_seat_in_the_plan()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var plan = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 1, 600));
        var tenantId = await TenantAsync(db, plan.Id);

        await using var scoped = Db(tenantId);
        var scopedService = new SubscriptionService(scoped);

        var student = new Core.Modules.Auth.Entities.Student
        {
            TenantId = tenantId,
            FullName = "Solo",
            Email = $"solo-{Guid.NewGuid():N}@test.local",
            PasswordHash = "x"
        };

        scoped.Students.Add(student);
        await scoped.SaveChangesAsync();

        await Assert.ThrowsAsync<LimitExceededException>(() => scopedService.EnsureCanAddStudentsAsync(1));

        // Silinmiş (arxivlənmiş) tələbə limiti tutmur.
        student.DeletedAt = DateTimeOffset.UtcNow;
        await scoped.SaveChangesAsync();

        await scopedService.EnsureCanAddStudentsAsync(1);
    }

    [Fact]
    public async Task The_student_limit_blocks_the_next_student()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var plan = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 50, 600));
        var tenantId = await TenantAsync(db, plan.Id);

        await using var scoped = Db(tenantId);
        var scopedService = new SubscriptionService(scoped);

        // 50 nəfər yarat.
        for (var i = 0; i < 50; i++)
        {
            scoped.Students.Add(new Core.Modules.Auth.Entities.Student
            {
                TenantId = tenantId,
                FullName = $"S{i}",
                Email = $"s{i}-{Guid.NewGuid():N}@test.local",
                PasswordHash = "x"
            });
        }

        await scoped.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<LimitExceededException>(() => scopedService.EnsureCanAddStudentsAsync(1));
        Assert.Contains("limitinə çatmısınız", error.Message);
    }

    [Fact]
    public async Task An_addon_raises_the_student_limit()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var plan = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 50, 600));
        var tenantId = await TenantAsync(db, plan.Id);

        await using var scoped = Db(tenantId);
        var scopedService = new SubscriptionService(scoped);

        await scopedService.AddAddonAsync(tenantId, new AddAddonRequest { Kind = AddonKinds.Students, Quantity = 20 });

        var current = await scopedService.GetAsync(tenantId);

        // 50 + 20 = 70
        Assert.Equal(70, current.Usage.MaxStudents);

        // Limit artıq 70-dir — 50-də bloklanmır.
        await scopedService.EnsureCanAddStudentsAsync(1);
    }

    [Fact]
    public async Task Any_quantity_of_an_addon_can_be_bought()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var plan = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 50, 600));
        var tenantId = await TenantAsync(db, plan.Id);

        await using var scoped = Db(tenantId);
        var scopedService = new SubscriptionService(scoped);

        // Paket anlayışı yoxdur — istənilən sayda tələbə yeri alınır.
        var addon = await scopedService.AddAddonAsync(tenantId,
            new AddAddonRequest { Kind = AddonKinds.Students, Quantity = 500 });

        Assert.Equal(500, addon.Quantity);
        Assert.Equal(500 * 8m, addon.Amount);

        var current = await scopedService.GetAsync(tenantId);
        Assert.Equal(550, current.Usage.MaxStudents);
    }

    [Fact]
    public async Task An_expired_plan_only_allows_reading()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var plan = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 50, 600));
        var tenantId = await TenantAsync(db, plan.Id, DateTimeOffset.UtcNow.AddDays(-2));

        await using var scoped = Db(tenantId);
        var scopedService = new SubscriptionService(scoped);

        var current = await scopedService.GetAsync(tenantId);

        Assert.Equal("readonly", current.Usage.Status);
        await Assert.ThrowsAsync<ForbiddenException>(() => scopedService.EnsureWritableAsync(tenantId));
    }

    [Fact]
    public async Task A_plan_expired_long_ago_is_suspended()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var plan = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 50, 600));
        var tenantId = await TenantAsync(db, plan.Id, DateTimeOffset.UtcNow.AddDays(-30));

        await using var scoped = Db(tenantId);
        var status = (await new SubscriptionService(scoped).GetAsync(tenantId)).Usage.Status;

        Assert.Equal("suspended", status);
    }

    [Fact]
    public async Task Video_minutes_are_recorded_in_the_current_period()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var plan = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 50, 600));
        var tenantId = await TenantAsync(db, plan.Id);

        await using var scoped = Db(tenantId);
        var scopedService = new SubscriptionService(scoped);

        await scopedService.RecordVideoMinutesAsync(tenantId, 120);

        var usage = await scopedService.GetAsync(tenantId);
        Assert.Equal(120, usage.Usage.VideoMinutes);
    }

    [Fact]
    public async Task The_video_limit_blocks_new_uploads_once_it_is_full()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var plan = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 50, 60));
        var tenantId = await TenantAsync(db, plan.Id);

        await using var scoped = Db(tenantId);
        var scopedService = new SubscriptionService(scoped);

        await scopedService.RecordVideoMinutesAsync(tenantId, 60);

        await Assert.ThrowsAsync<LimitExceededException>(() => scopedService.EnsureCanUploadAsync());
    }

    [Fact]
    public async Task A_plan_can_forbid_uploads_of_a_material_kind()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var request = Request($"p-{Guid.NewGuid():N}"[..20], 50, 600);
        request.AllowVideoUpload = false;

        var plan = await service.CreatePlanAsync(request);
        Assert.False(plan.AllowVideoUpload);
        Assert.True(plan.AllowFileUpload);

        var tenantId = await TenantAsync(db, plan.Id);

        await using var scoped = Db(tenantId);
        var scopedService = new SubscriptionService(scoped);

        await Assert.ThrowsAsync<ForbiddenException>(() => scopedService.EnsureCanUploadAsync(UploadKind.Video));
        await scopedService.EnsureCanUploadAsync(UploadKind.File);
        await scopedService.EnsureCanUploadAsync(UploadKind.Audio);
    }
}

/// <summary>Bucket taraması: tenant-ın tutduğu yer və köhnə orijinalların silinməsi.</summary>
[Collection("api")]
public class StorageMeteringTests(ApiFactory factory)
{
    private sealed class FixedTenant(long tenantId) : ITenantProvider
    {
        public long? TenantId => tenantId;

        public bool IgnoreTenant => false;
    }

    private AppDbContext Db(long tenantId = 0)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(factory.ConnectionString)
            .Options;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Tenancy:Strict"] = "false" })
            .Build();

        return new AppDbContext(options, new FixedTenant(tenantId), config);
    }

    [Fact]
    public async Task Metering_records_the_bytes_a_tenant_uses()
    {
        await using var db = Db();

        var tenant = new Core.Modules.Tenant.Entities.Tenant
        {
            Name = $"M-{Guid.NewGuid():N}",
            Slug = $"m-{Guid.NewGuid():N}",
            IsActive = true
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        using var scope = factory.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<Edutor.Core.Common.Storage.IFileStorage>();
        var folders = scope.ServiceProvider.GetRequiredService<Edutor.Core.Common.Storage.ITenantFolder>();

        var folder = await folders.ResolveAsync(tenant.Id);
        await storage.UploadTextAsync($"{folder}/videos/raw/test.mp4", new string('x', 1000));
        await storage.UploadTextAsync($"{folder}/files/doc.pdf", new string('y', 500));

        var metering = scope.ServiceProvider.GetRequiredService<IStorageMeteringService>();
        await metering.MeterAsync();

        await using var check = Db();
        var usage = await check.TenantUsages.IgnoreQueryFilters()
            .FirstAsync(x => x.TenantId == tenant.Id);

        Assert.True(usage.StorageBytes >= 1500, $"Ölçülən: {usage.StorageBytes}");
    }

    [Fact]
    public async Task Hls_output_is_not_counted_but_originals_are()
    {
        await using var db = Db();

        var tenant = new Core.Modules.Tenant.Entities.Tenant
        {
            Name = $"H-{Guid.NewGuid():N}",
            Slug = $"h-{Guid.NewGuid():N}",
            IsActive = true
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        using var scope = factory.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<Edutor.Core.Common.Storage.IFileStorage>();
        var folders = scope.ServiceProvider.GetRequiredService<Edutor.Core.Common.Storage.ITenantFolder>();

        var folder = await folders.ResolveAsync(tenant.Id);
        var raw = $"{folder}/videos/raw/lesson.mp4";
        await storage.UploadTextAsync(raw, new string('x', 4000));
        await storage.UploadTextAsync($"{folder}/videos/m3u8/lesson/v1/master.m3u8", new string('y', 300));
        await storage.UploadTextAsync($"{folder}/videos/m3u8/lesson/v1/720/index.m3u8", new string('z', 5000));
        await storage.UploadTextAsync($"{folder}/videos/m3u8/lesson/v1/720/seg0.ts", new string('s', 9000));

        var metering = scope.ServiceProvider.GetRequiredService<IStorageMeteringService>();
        await metering.MeterAsync();

        await using var check = Db();
        var usage = await check.TenantUsages.IgnoreQueryFilters()
            .FirstAsync(x => x.TenantId == tenant.Id);

        // Yalnız orijinal video — HLS playlistləri və seqmentləri sayılmır.
        Assert.Equal(await storage.GetSizeAsync(raw), usage.StorageBytes);
    }

    [Fact]
    public async Task Expired_originals_are_deleted_only_when_enabled()
    {
        await using var db = Db();

        var service = new SubscriptionService(db);
        var plan = await service.CreatePlanAsync(new PlanRequest
        {
            Code = $"r-{Guid.NewGuid():N}"[..20],
            Name = "Retention",
            MonthlyPrice = 10,
            MaxStudents = 10,
            MaxGroups = 5,
            MonthlyVideoMinutes = 60,
            StorageGb = 1,
            ConcurrentConversions = 1,
            OriginalRetentionDays = 1,
            IsActive = true,
            SortOrder = 99
        });

        var tenant = new Core.Modules.Tenant.Entities.Tenant
        {
            Name = $"R-{Guid.NewGuid():N}",
            Slug = $"r-{Guid.NewGuid():N}",
            IsActive = true,
            PlanId = plan.Id
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        using var scope = factory.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<Edutor.Core.Common.Storage.IFileStorage>();
        var folders = scope.ServiceProvider.GetRequiredService<Edutor.Core.Common.Storage.ITenantFolder>();

        var folder = await folders.ResolveAsync(tenant.Id);
        var key = $"{folder}/videos/raw/old.mp4";
        await storage.UploadTextAsync(key, "old");

        await using (var videoDb = Db(tenant.Id))
        {
            var video = new Core.Modules.Media.Entities.Video
            {
                TenantId = tenant.Id,
                Title = "old",
                Path = key,
                ConversionStatus = Core.Modules.Media.Entities.VideoStatus.Ready,
                PlaybackUrl = $"{folder}/videos/m3u8/1/v1/master.m3u8"
            };

            videoDb.Videos.Add(video);
            await videoDb.SaveChangesAsync();

            // CreatedAt audit tərəfindən "indi" yazılır — köhnəltmək üçün ikinci save.
            video.CreatedAt = DateTimeOffset.UtcNow.AddDays(-10);
            await videoDb.SaveChangesAsync();
        }

        var metering = scope.ServiceProvider.GetRequiredService<IStorageMeteringService>();

        // Söndürülmüş halda silinmir.
        await metering.MeterAsync();
        Assert.True(await storage.ExistsAsync(key, ct: default));

        // Yandırılmış halda silinir.
        var enabled = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Core.Modules.Subscription.SubscriptionOptions>>();
        enabled.Value.DeleteExpiredOriginals = true;

        await metering.MeterAsync();
        Assert.False(await storage.ExistsAsync(key, ct: default));
    }
}
