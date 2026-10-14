using Kela.Core.Modules.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kela.Core.Data;

public static class DbSeeder
{
    /// <summary>Heç bir superadmin yoxdursa, konfiqurasiyadan default superadmin yaradır (bootstrap).</summary>
    public static async Task SeedDefaultAdminAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Type == UserTypes.SuperAdmin, ct)) return;

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
}
