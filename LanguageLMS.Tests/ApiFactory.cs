using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LanguageLMS.Core.Data;
using UserEntity = LanguageLMS.Core.Modules.User.Entities.User;
using UserTypes = LanguageLMS.Core.Modules.User.Entities.UserTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace LanguageLMS.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public string AdminToken { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // admin seed (test üçün)
        db.Users.Add(new UserEntity
        {
            Name = "Admin",
            Email = "admin@test.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Type = UserTypes.Admin
        });
        await db.SaveChangesAsync();

        var client = CreateClient();
        var login = await client.PostAsJsonAsync("/api/user-auth/login", new { email = "admin@test.local", password = "admin123" });
        var doc = await JsonDocument.ParseAsync(await login.Content.ReadAsStreamAsync());
        AdminToken = doc.RootElement.GetProperty("data").GetProperty("token").GetString()!;
    }

    /// <summary>Admin (bütün teacher uçlarına da girişi var) ilə autentifikasiyalı client.</summary>
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
                ["ConnectionStrings:Postgres"] = _db.GetConnectionString()
            });
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _db.DisposeAsync();
        Dispose();
    }
}

[CollectionDefinition("api")]
public sealed class ApiCollection : ICollectionFixture<ApiFactory> { }
