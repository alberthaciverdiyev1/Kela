using Kela.Api.Middleware;
using Kela.Application;
using Kela.Domain.Tenants;
using Kela.Domain.Tenants.Enums;
using Kela.Infrastructure;
using Kela.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Cookie-only kimlik doğrulama:
//   tenant_id claim'i login'de cookie'ye yazılır, TenantResolutionMiddleware okur.
//   SetApplicationName sabit tutulur → API ile Web (Kela.Web) aynı cookie'yi paylaşabilir.
//   KeysPath verilirse key'ler dosyada saklanır → çoklu node / API+Web aynı key ring'i kullanır.
builder.Services
    .AddDataProtection()
    .SetApplicationName("Kela")
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeysPath"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "keys")));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Kela.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // API: 401/403'ü login sayfasına redirect yerine durum kodu olarak dön
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();
app.MapControllers();

// Geliştirme ortamı: migration'ları uygula + default tenant oluştur (test kolaylığı)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KelaDbContext>();

    if (app.Environment.IsDevelopment())
    {
        db.Database.Migrate();

        if (!db.Tenants.Any(t => t.Slug == "default"))
        {
            db.Tenants.Add(new Tenant
            {
                Id = 0, // EF identity DB'de üretir
                Name = "Default Tenant",
                Slug = "default",
                Status = TenantStatus.Active,
                CreatedAt = DateTime.UtcNow,
            });
            db.SaveChanges();
        }
    }
}

app.Run();
