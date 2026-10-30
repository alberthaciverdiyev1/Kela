using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;
using Edutor.Core;
using Edutor.Core.Common.Health;
using Edutor.Core.Common.Http;
using Edutor.Core.Common.Localization;
using Edutor.Core.Common.Logging;
using Microsoft.AspNetCore.DataProtection;
using Edutor.Core.Data;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Subscription;
using Edutor.Admin.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

var builder = WebApplication.CreateBuilder(args);
builder.UseEdutorLogging("admin");

builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
    // Plan seçməmiş tenant admini hər səhifədən plan seçiminə yönləndirilir.
    options.Filters.AddService<PlanSelectionFilter>();
    // Fərdi müəllim tək işləyir — istifadəçi idarəetməsi bağlıdır.
    options.Filters.AddService<IndividualTeacherGuardFilter>();
});
builder.Services.AddScoped<PlanSelectionFilter>();
builder.Services.AddScoped<IndividualTeacherGuardFilter>();
builder.Services.AddCore(builder.Configuration);
builder.Services.AddEdutorReverseProxy();

// Cookie-lər restart-da qalmalıdır: anahtarlar fayl sistemində saxlanılır.
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("Edutor.Admin");
var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}
builder.Services.AddEdutorLocalization();

// Public qeydiyyat və digər anonim formlar üçün sadə rate limit.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Login brute-force: IP üzrə dəqiqədə 10 cəhd.
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "anon",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("public", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "anon",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // Session cookie yalnız HTTP-only, Lax və HTTPS-də göndərilir.
        // Development-da http://localhost işlədiyi üçün Secure tələb olunmur.
        options.Cookie.Name = "edutor.admin.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "edutor.admin.antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// Yalnız personel (teacher/admin/superadmin) MVC-yə girə bilər; student yalnız API-yə.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.Teacher, p => p.RequireRole(AuthRoles.Teacher, AuthRoles.Admin, AuthRoles.SuperAdmin));
    options.AddPolicy(AuthPolicies.Admin, p => p.RequireRole(AuthRoles.Admin, AuthRoles.SuperAdmin));
    options.AddPolicy(AuthPolicies.SuperAdmin, p => p.RequireRole(AuthRoles.SuperAdmin));

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole(AuthRoles.Teacher, AuthRoles.Admin, AuthRoles.SuperAdmin)
        .Build();
});

Edutor.Core.Common.Configuration.SecretGuard.EnsureProductionSecrets(builder.Configuration, builder.Environment.IsDevelopment());

var app = builder.Build();

app.UseEdutorReverseProxy(app.Configuration);

await app.Services.MigrateAndSeedAsync(app.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/home/error");
    app.UseHsts();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// TLS-i nginx həll edir — proxy arxasında yönləndirmə heç vaxt işləmir, yalnız xəbərdarlıq yazır.
if (!app.Configuration.GetValue<bool>("ReverseProxy:Enabled")) app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseCoreStorage();

app.UseEdutorLocalization();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapStaticAssets();

app.MapGet("/", () => Results.Redirect("/dashboard"));

app.MapEdutorHealth();
app.MapControllers();

app.Run();
