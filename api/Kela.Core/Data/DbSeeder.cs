using Kela.Core.Modules.Auth.Entities;
using Kela.Core.Modules.Tenant.Entities;
using Kela.Core.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kela.Core.Data;

public static class DbSeeder
{
    /// <summary>
    /// Bootstrap: default tenant, superadmin və hər tenant üçün default student/admin hesabları yaradır.
    /// Parollar/emaillər konfiqurasiyadan oxunur (bax: "DefaultAccounts" bölməsi).
    /// </summary>
    public static async Task SeedDefaultAdminAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        // Default tenant yoxdursa yaradılır (login səhifəsində görünsün).
        if (!await db.Tenants.AnyAsync(x => x.Slug == "default", ct))
        {
            db.Tenants.Add(new Tenant { Name = "Default", Slug = "default" });
            await db.SaveChangesAsync(ct);
            logger.LogWarning("Default tenant yaradıldı.");
        }

        // Superadmin (qlobal, tenant-sız).
        if (!await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Type == UserTypes.SuperAdmin, ct))
        {
            var email = Setting(configuration, "Admin:Email", "admin@kela.local");
            var password = Setting(configuration, "Admin:Password", "admin123");

            db.Users.Add(new User
            {
                Name = "SuperAdmin",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Type = UserTypes.SuperAdmin
            });

            await db.SaveChangesAsync(ct);
            logger.LogWarning("Default superadmin yaradıldı: {Email}", email);
        }

        await SeedSampleTenantsAsync(db, configuration, logger, ct);
        await SeedTenantAccountsAsync(db, configuration, logger, ct);
    }

    /// <summary>Yalnız təzə quraşdırmada (≤1 tenant) bir neçə nümunə kurs yaradır.</summary>
    private static async Task SeedSampleTenantsAsync(AppDbContext db, IConfiguration configuration, ILogger logger, CancellationToken ct)
    {
        if (!(configuration.GetValue<bool?>("Seed:SampleTenants") ?? true)) return;
        if (await db.Tenants.CountAsync(ct) > 1) return;

        var samples = new (string Name, string Slug)[]
        {
            ("İngilis Dili", "ingilis-dili"),
            ("Türk Dili", "turk-dili"),
            ("Proqramlaşdırma", "programlasdirma")
        };

        foreach (var (name, slug) in samples)
        {
            if (await db.Tenants.AnyAsync(t => t.Slug == slug, ct)) continue;
            db.Tenants.Add(new Tenant { Name = name, Slug = slug });
        }

        await db.SaveChangesAsync(ct);
        logger.LogWarning("Nümunə tenant-lar yaradıldı ({Count}).", samples.Length);
    }

    /// <summary>Hər tenant üçün default student və admin hesablarını yaradır (idempotent).</summary>
    private static async Task SeedTenantAccountsAsync(AppDbContext db, IConfiguration configuration, ILogger logger, CancellationToken ct)
    {
        var studentEmail = Setting(configuration, "DefaultAccounts:Student:Email", "student1@kela.az");
        var studentPassword = Setting(configuration, "DefaultAccounts:Student:Password", "student123");
        var adminEmail = Setting(configuration, "DefaultAccounts:Admin:Email", "admin@kela.az");
        var adminPassword = Setting(configuration, "DefaultAccounts:Admin:Password", "admin123");

        var tenantIds = await db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync(ct);
        var added = 0;

        foreach (var tenantId in tenantIds)
        {
            if (!await db.Students.IgnoreQueryFilters()
                    .AnyAsync(s => s.TenantId == tenantId && s.Email == studentEmail, ct))
            {
                db.Students.Add(new Student
                {
                    TenantId = tenantId,
                    FullName = "Test Student",
                    Email = studentEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(studentPassword),
                    IsActive = true
                });
                added++;
            }

            if (!await db.Users.IgnoreQueryFilters()
                    .AnyAsync(u => u.TenantId == tenantId && u.Email == adminEmail, ct))
            {
                db.Users.Add(new User
                {
                    TenantId = tenantId,
                    Name = "Kurs",
                    Surname = "Admin",
                    Email = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Type = UserTypes.Admin
                });
                added++;
            }
        }

        if (added > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogWarning("{Count} default tenant hesabı yaradıldı ({Student} / {Admin}).", added, studentEmail, adminEmail);
        }
    }

    // Boş dəyər "konfiqurasiya olunmayıb" sayılır — appsettings-də boş qalmış
    // parol fallback-ı söndürməsin, boş parollu hesab yaranmasın.
    private static string Setting(IConfiguration configuration, string key, string fallback)
        => string.IsNullOrWhiteSpace(configuration[key]) ? fallback : configuration[key]!;
}
