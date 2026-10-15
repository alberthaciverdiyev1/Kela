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
            var email = configuration["Admin:Email"] ?? "superadmin@kela.local";
            var password = configuration["Admin:Password"] ?? "admin123";

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

        await SeedTenantAccountsAsync(db, configuration, logger, ct);
    }

    /// <summary>Hər tenant üçün default student və admin hesablarını yaradır (idempotent).</summary>
    private static async Task SeedTenantAccountsAsync(AppDbContext db, IConfiguration configuration, ILogger logger, CancellationToken ct)
    {
        var studentEmail = configuration["DefaultAccounts:Student:Email"] ?? "student1@kela.az";
        var studentPassword = configuration["DefaultAccounts:Student:Password"] ?? "student123";
        var adminEmail = configuration["DefaultAccounts:Admin:Email"] ?? "admin@kela.az";
        var adminPassword = configuration["DefaultAccounts:Admin:Password"] ?? "admin123";

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
}
