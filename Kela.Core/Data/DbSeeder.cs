using Kela.Core.Modules.Permission.Entities;
using Kela.Core.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kela.Core.Data;

public static class DbSeeder
{
    /// <summary>Heç bir istifadəçi yoxdursa, konfiqurasiyadan default admin yaradır (bootstrap).</summary>
    public static async Task SeedDefaultAdminAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Type == UserTypes.Admin, ct)) return;

        var email = configuration["Admin:Email"] ?? "admin@kela.local";
        var password = configuration["Admin:Password"] ?? "admin123";

        db.Users.Add(new User
        {
            Name = "Admin",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Type = UserTypes.Admin
        });

        await db.SaveChangesAsync(ct);
        logger.LogWarning("Default admin yaradıldı: {Email}", email);
    }

    public static readonly string[] DefaultPermissions =
    [
        "banner.manage", "subject.manage", "lesson.manage", "group.manage",
        "question.manage", "quiz.manage", "user.manage", "notification.manage", "setting.manage"
    ];

    /// <summary>İcazə cədvəli boşdursa, standart icazələri yaradır.</summary>
    public static async Task SeedDefaultPermissionsAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.Permissions.IgnoreQueryFilters().AnyAsync(ct)) return;

        foreach (var name in DefaultPermissions)
        {
            db.Permissions.Add(new Permission { Name = name });
        }

        await db.SaveChangesAsync(ct);
    }
}
