using Edutor.Core.Modules.Subscription;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Core.Data;

public static class DatabaseInitializer
{
    // Sıra vacibdir: əvvəl sxem (migration), sonra seed — təzə bazada seed cədvəlsiz
    // işləyib "relation does not exist" xətası verməsin.
    public static async Task MigrateAndSeedAsync(
        this IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        if (configuration.GetValue<bool>("Database:MigrateOnStartup"))
        {
            using var scope = services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync(ct);
        }

        await services.SeedDefaultAdminAsync(configuration, ct);
        await services.SeedSubscriptionPlansAsync(ct);
    }
}
