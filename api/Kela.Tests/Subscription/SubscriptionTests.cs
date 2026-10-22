using Kela.Core.Common.Exceptions;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Data;
using Kela.Core.Modules.Subscription;
using Kela.Core.Modules.Subscription.Entities;
using Kela.Core.Modules.Subscription.Requests;
using Kela.Core.Modules.Subscription.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kela.Tests.Subscription;

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

    private static PlanRequest Request(string code, int maxStudents, int monthlyVideoMinutes, int maxAddonPercent = 100) => new()
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
        AddonStudentPackSize = 10,
        AddonStudentPackPrice = 8,
        AddonVideoPackMinutes = 600,
        AddonVideoPackPrice = 12,
        AddonStoragePackGb = 50,
        AddonStoragePackPrice = 10,
        MaxAddonPercent = maxAddonPercent,
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

        var error = await Assert.ThrowsAsync<ForbiddenException>(() => scopedService.EnsureCanAddStudentsAsync(1));
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

        await scopedService.AddAddonAsync(tenantId, new AddAddonRequest { Kind = AddonKinds.Students, Packs = 2 });

        var current = await scopedService.GetAsync(tenantId);

        // 50 + 2 × 10 = 70
        Assert.Equal(70, current.Usage.MaxStudents);

        // Limit artıq 70-dir — 50-də bloklanmır.
        await scopedService.EnsureCanAddStudentsAsync(1);
    }

    [Fact]
    public async Task An_addon_cannot_exceed_the_plan_cap()
    {
        await using var db = Db(0);
        var service = new SubscriptionService(db);

        var plan = await service.CreatePlanAsync(Request($"p-{Guid.NewGuid():N}"[..20], 50, 600, maxAddonPercent: 100));
        var tenantId = await TenantAsync(db, plan.Id);

        await using var scoped = Db(tenantId);
        var scopedService = new SubscriptionService(scoped);

        // 50 limitin 100%-i = 50 əlavə tələbə = 5 paket.
        await scopedService.AddAddonAsync(tenantId, new AddAddonRequest { Kind = AddonKinds.Students, Packs = 5 });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            scopedService.AddAddonAsync(tenantId, new AddAddonRequest { Kind = AddonKinds.Students, Packs = 1 }));
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

        await Assert.ThrowsAsync<ForbiddenException>(() => scopedService.EnsureCanUploadAsync());
    }
}
