using Kela.Application.Abstractions.Security;
using Kela.Application.Abstractions.Tenancy;
using Kela.Application.Repositories;
using Kela.Infrastructure.Data;
using Kela.Infrastructure.Repositories;
using Kela.Infrastructure.Security;
using Kela.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("'ConnectionStrings:Postgres' yapılandırması bulunamadı.");

        // Multi-tenant altyapısı
        // Tenant'ı Api/Web katmanındaki middleware SetTenant ile besler.
        services.AddScoped<ICurrentTenant, TenantContext>();
        services.AddScoped<TenantSaveChangesInterceptor>();

        services.AddDbContext<KelaDbContext>((sp, options) =>
            options.UseNpgsql(connectionString)
                   .AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>()));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGradeRepository, GradeRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<KelaDbContext>());
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}
