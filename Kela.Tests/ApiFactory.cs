using System.Net.Http.Headers;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Data;
using Kela.Core.Modules.Auth.Abstractions;
using UserTypes = Kela.Core.Modules.User.Entities.UserTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Kela.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public string AdminToken { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        // Program startup-da seed edir, ona görə host işə düşməzdən ƏVVƏL migrasiyaları tətbiq et.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_db.GetConnectionString())
            .Options;
        await using (var db = new AppDbContext(options, new NullTenantProvider(), new ConfigurationBuilder().Build()))
        {
            await db.Database.MigrateAsync();
        }

        // Host-ı işə sal (Program admin + icazələri seed edir) və admin tokeni yarat.
        using var scope = Services.CreateScope();
        var admin = await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .Users.IgnoreQueryFilters().FirstAsync(u => u.Type == UserTypes.SuperAdmin);

        var tokens = scope.ServiceProvider.GetRequiredService<ITokenService>();
        AdminToken = tokens.CreateToken(admin.Id, admin.Email, admin.Name ?? "SuperAdmin", "superadmin", 0);
    }

    /// <summary>Admin tokeni ilə client — şagird olmayan istifadəçi yoxlamaları üçün.</summary>
    public HttpClient TeacherClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _db.GetConnectionString(),
                ["RateLimit:AuthPermitLimit"] = "1000000",
                ["Tenancy:Strict"] = "false"
            });
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _db.DisposeAsync();
        Dispose();
    }

    private sealed class NullTenantProvider : ITenantProvider
    {
        public long? TenantId => null;

        public bool IgnoreTenant => false;
    }
}

[CollectionDefinition("api")]
public sealed class ApiCollection : ICollectionFixture<ApiFactory> { }
