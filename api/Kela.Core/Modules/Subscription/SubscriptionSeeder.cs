using Kela.Core.Data;
using Kela.Core.Modules.Subscription.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Core.Modules.Subscription;

/// <summary>
/// İlk quraşdırmada 4 standart planı yaradır. Planlar sonradan admin paneldən
/// dəyişdirilir — seeder yalnız boş cədvəli doldurur, mövcud planlara toxunmur.
/// </summary>
public static class SubscriptionSeeder
{
    private const long Gb = 1024L * 1024 * 1024;

    public static async Task SeedSubscriptionPlansAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.SubscriptionPlans.AnyAsync(ct)) return;

        db.SubscriptionPlans.AddRange(Plans());
        await db.SaveChangesAsync(ct);
    }

    private static IEnumerable<SubscriptionPlan> Plans()
    {
        yield return new SubscriptionPlan
        {
            Code = "basic", Name = "Basic", Description = "Fərdi müəllim üçün başlanğıc",
            MonthlyPrice = 39, Currency = "AZN", SortOrder = 1,
            MaxTeachers = 1, MaxStudents = 50, MaxGroups = 5, MaxSubjects = 3, MaxLessons = 100,
            MonthlyVideoMinutes = 20 * 60, StorageBytes = 30 * Gb,
            ConcurrentConversions = 2, MaxVideoHeight = 720, OriginalRetentionDays = 30,
            AddonStudentPackSize = 10, AddonStudentPackPrice = 8,
            AddonVideoPackMinutes = 10 * 60, AddonVideoPackPrice = 12,
            AddonStoragePackBytes = 50 * Gb, AddonStoragePackPrice = 10,
            Features = "{}"
        };

        yield return new SubscriptionPlan
        {
            Code = "pro", Name = "Pro", Description = "Kiçik kurs üçün",
            MonthlyPrice = 99, Currency = "AZN", SortOrder = 2,
            MaxTeachers = 3, MaxStudents = 300, MaxGroups = 25, MaxSubjects = 10, MaxLessons = 500,
            MonthlyVideoMinutes = 80 * 60, StorageBytes = 150 * Gb,
            ConcurrentConversions = 4, MaxVideoHeight = 1080, OriginalRetentionDays = 90,
            AddonStudentPackSize = 10, AddonStudentPackPrice = 8,
            AddonVideoPackMinutes = 10 * 60, AddonVideoPackPrice = 12,
            AddonStoragePackBytes = 50 * Gb, AddonStoragePackPrice = 10,
            Features = """{"certificates":true,"payments":true}"""
        };

        yield return new SubscriptionPlan
        {
            Code = "premium", Name = "Premium", Description = "Qurulmuş kurs üçün",
            MonthlyPrice = 249, Currency = "AZN", SortOrder = 3,
            MaxTeachers = 10, MaxStudents = 1000, MaxGroups = 100, MaxSubjects = 50, MaxLessons = 2000,
            MonthlyVideoMinutes = 250 * 60, StorageBytes = 500 * Gb,
            ConcurrentConversions = 8, MaxVideoHeight = 1080, OriginalRetentionDays = 180,
            AddonStudentPackSize = 10, AddonStudentPackPrice = 8,
            AddonVideoPackMinutes = 10 * 60, AddonVideoPackPrice = 12,
            AddonStoragePackBytes = 50 * Gb, AddonStoragePackPrice = 10,
            Features = """{"certificates":true,"payments":true,"crm":true,"chat":true,"branding":true}"""
        };

        yield return new SubscriptionPlan
        {
            Code = "advanced", Name = "Advanced", Description = "Böyük təhsil müəssisəsi",
            MonthlyPrice = 599, Currency = "AZN", SortOrder = 4,
            MaxTeachers = 0, MaxStudents = 5000, MaxGroups = 0, MaxSubjects = 0, MaxLessons = 0,
            MonthlyVideoMinutes = 800 * 60, StorageBytes = 2 * 1024 * Gb,
            ConcurrentConversions = 16, MaxVideoHeight = 1080, OriginalRetentionDays = 0,
            AddonStudentPackSize = 10, AddonStudentPackPrice = 8,
            AddonVideoPackMinutes = 10 * 60, AddonVideoPackPrice = 12,
            AddonStoragePackBytes = 50 * Gb, AddonStoragePackPrice = 10,
            Features = """{"certificates":true,"payments":true,"crm":true,"chat":true,"branding":true,"api":true}"""
        };
    }
}
