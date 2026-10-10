using LanguageLMS.Core.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LanguageLMS.Core.Data;

public static class DbSeeder
{
    /// <summary>Heç bir istifadəçi yoxdursa, konfiqurasiyadan default admin yaradır (bootstrap).</summary>
    public static async Task SeedDefaultAdminAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Type == UserTypes.Admin, ct)) return;

        var email = configuration["Admin:Email"] ?? "admin@languagelms.local";
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
}
