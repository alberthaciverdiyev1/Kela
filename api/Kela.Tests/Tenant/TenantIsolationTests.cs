using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Kela.Core.Common.MultiTenancy;
using Kela.Core.Data;
using Kela.Core.Modules.Subject.Entities;
using SubjectEntity = Kela.Core.Modules.Subject.Entities.Subject;
using Kela.Core.Modules.Tenant.Entities;
using TenantEntity = Kela.Core.Modules.Tenant.Entities.Tenant;
using Kela.Core.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Tests.MultiTenancy;

[Collection("api")]
public class TenantIsolationTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_is_scoped_to_the_selected_tenant()
    {
        var (tenantA, email, passwordA, passwordB) = await SeedTwoTenantsAsync();

        var loginA = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = passwordA, tenantId = tenantA });
        Assert.Equal(HttpStatusCode.OK, loginA.StatusCode);
        Assert.Equal(tenantA, TenantClaimOf(await loginA.Content.ReadAsStringAsync()));

        // Eyni email başqa tenant-da fərqli şifrəyə malikdir → A şifrəsi ilə daxil olmaq mümkün deyil.
        var wrongTenant = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = passwordA, tenantId = tenantA + 1 });
        Assert.Equal(HttpStatusCode.Unauthorized, wrongTenant.StatusCode);
    }

    [Fact]
    public async Task Query_filter_hides_other_tenants_data()
    {
        var (tenantA, _, _, _) = await SeedTwoTenantsAsync();
        var tenantB = tenantA + 1;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(factory.ConnectionString).Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Tenancy:Strict"] = "true" }).Build();

        // Tenant A üçün subject yarat
        var nameA = $"S-{Guid.NewGuid():N}";
        await using (var dbA = new AppDbContext(options, new FixedTenantProvider(tenantA), config))
        {
            dbA.Subjects.Add(new SubjectEntity { TenantId = tenantA, Name = nameA });
            await dbA.SaveChangesAsync();
        }

        await using var scopedA = new AppDbContext(options, new FixedTenantProvider(tenantA), config);
        await using var scopedB = new AppDbContext(options, new FixedTenantProvider(tenantB), config);

        Assert.Contains(await scopedA.Subjects.ToListAsync(), s => s.Name == nameA);
        Assert.DoesNotContain(await scopedB.Subjects.ToListAsync(), s => s.Name == nameA);
    }

    private async Task<(long tenantA, string email, string passwordA, string passwordB)> SeedTwoTenantsAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(factory.ConnectionString).Options;
        var config = new ConfigurationBuilder().Build();
        var provider = new FixedTenantProvider(null);

        var email = $"student-{Guid.NewGuid():N}@ex.com";
        const string passwordA = "secret123";
        const string passwordB = "other456";

        await using var db = new AppDbContext(options, provider, config);

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var a = new TenantEntity { Name = $"A-{suffix}", Slug = $"a-{suffix}" };
        var b = new TenantEntity { Name = $"B-{suffix}", Slug = $"b-{suffix}" };
        db.Tenants.AddRange(a, b);
        await db.SaveChangesAsync();

        db.Students.Add(new Student { TenantId = a.Id, FullName = "A", Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordA), IsActive = true });
        db.Students.Add(new Student { TenantId = b.Id, FullName = "B", Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordB), IsActive = true });
        await db.SaveChangesAsync();

        return (a.Id, email, passwordA, passwordB);
    }

    private static long TenantClaimOf(string json)
    {
        var token = JsonDocument.Parse(json).RootElement.GetProperty("data").GetProperty("token").GetString()!;
        var payload = token.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var claims = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload))).RootElement;
        return long.Parse(claims.GetProperty("tenant_id").GetString()!);
    }

    private sealed class FixedTenantProvider(long? tenantId) : ITenantProvider
    {
        public long? TenantId => tenantId;
        public bool IgnoreTenant => false;
    }
}

[Collection("api")]
public class TenantDeleteTests(ApiFactory factory)
{
    [Fact]
    public async Task Deleting_a_tenant_hides_it_and_blocks_it_from_the_list()
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<Kela.Core.Modules.Tenant.Abstractions.ITenantService>();

        var created = await service.CreateAsync(new Kela.Core.Modules.Tenant.Requests.CreateTenantRequest
        {
            Name = $"Delete {Guid.NewGuid():N}",
            Slug = $"del-{Guid.NewGuid():N}"
        });

        await service.DeleteAsync(created.Id);

        Assert.Null(await service.GetByIdAsync(created.Id));
        Assert.DoesNotContain(await service.ListAsync(), x => x.Id == created.Id);
    }
}

[Collection("api")]
public class TenantRegistrationTests(ApiFactory factory)
{
    [Fact]
    public async Task An_individual_teacher_gets_their_own_tenant_and_the_teacher_role()
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<Kela.Core.Modules.Tenant.Abstractions.ITenantService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tenant = await service.RegisterAsync(new Kela.Core.Modules.Tenant.Requests.RegisterTenantRequest
        {
            AccountType = "teacher",
            AdminName = "Aysel",
            AdminSurname = "Aliyeva",
            Email = $"teacher-{Guid.NewGuid():N}@test.local",
            PhoneNumber = "0500000000",
            Password = "Password1",
            ConfirmPassword = "Password1",
            AcceptTerms = true
        });

        Assert.Equal("Aysel Aliyeva", tenant.Name);

        var user = await db.Users.IgnoreQueryFilters().FirstAsync(u => u.TenantId == tenant.Id);
        Assert.Equal("teacher", user.Type);
    }

    [Fact]
    public async Task A_course_registration_creates_an_admin_and_requires_a_phone()
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<Kela.Core.Modules.Tenant.Abstractions.ITenantService>();

        await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterAsync(
            new Kela.Core.Modules.Tenant.Requests.RegisterTenantRequest
            {
                AccountType = "course",
                CourseName = $"Kurs {Guid.NewGuid():N}",
                AdminName = "Test",
                Email = $"admin-{Guid.NewGuid():N}@test.local",
                Password = "Password1",
                ConfirmPassword = "Password1",
                AcceptTerms = true
            }));
    }
}
