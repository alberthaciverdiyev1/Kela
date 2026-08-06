using Kela.Api.Endpoints;
using Kela.Api.Middleware;
using Kela.Application;
using Kela.Infrastructure;
using Kela.Infrastructure.Data;
using Kela.Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
// .NET 10'da AddExceptionHandler<T> artık AddProblemDetails'i ÇAĞIRMAZ;
// parametresiz UseExceptionHandler() IProblemDetailsService ister — bu yüzden açıkça eklenir.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Cookie-only kimlik doğrulama:
//   SetApplicationName sabit tutulur → API ile Web (Kela.Web) aynı cookie'yi paylaşabilir.
//   KeysPath verilirse key'ler dosyada saklanır → çoklu node / API+Web aynı key ring'i kullanır.
builder.Services
    .AddDataProtection()
    .SetApplicationName("Kela")
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeysPath"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "keys")));

builder.Services.AddAuthorization();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// ASP.NET Core Identity cookie'si (IdentityConstants.ApplicationScheme).
// DİKKAT: AddInfrastructure'dan SONRA çağrılmalıdır — AddIdentity'nin AddCookie configure'u
// Events'i yeni bir nesneyle değiştirir (OnValidatePrincipal dahil). Burada o nesneyi
// MUTASYON ederiz; böylece hem Identity'nin security-stamp hook'u hem API için
// redirect yerine 401/403 davranışı birlikte korunur.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Kela.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapSectionsEndpoints();
app.MapUsersEndpoints();
app.MapAuthEndpoints();
app.MapSiteConfigurationEndpoints();
app.MapCitiesEndpoints();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<KelaDbContext>();

    if (app.Environment.IsDevelopment())
    {
        db.Database.Migrate();
    }

    // Sabit Identity rolleri: Admin/Teacher/Student/Parent (AspNetRoles).
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    foreach (var roleName in new[] { "Admin", "Teacher", "Student", "Parent" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }

    // Başlangıç şehirleri (dump): Henüz Admin yok; tablo boşsa 4 dildeki
    // örnek şehirler eklenir. CRUD arayüzü gelince Admin buradan yönetir.
    if (!await db.Cities.AnyAsync())
    {
        db.Cities.AddRange(CitySeed.Build());
        await db.SaveChangesAsync();
    }
}

app.Run();
