using System.Text;
using System.Threading.RateLimiting;
using Edutor.Core.Data;
using Microsoft.EntityFrameworkCore;
using Edutor.Api.Common.Middleware;
using Edutor.Api.Common.Routing;
using Edutor.Api.Common.Responses;
using Edutor.Api.Common.Validation;
using Edutor.Api.Modules.Nodes.Auth;
using Edutor.Api.Modules.Nodes.Services;
using Edutor.Core;
using Edutor.Core.Common.Health;
using Edutor.Core.Common.Http;
using Edutor.Core.Common.Localization;
using Edutor.Core.Common.Logging;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Subscription;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);
builder.UseEdutorLogging("api");

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .ToDictionary(
                kv => kv.Key,
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        return new BadRequestObjectResult(ApiResponse.Error(localizer["Api_ValidationFailed"], 400, errors: errors));
    };
});

builder.Services.AddOpenApi();

// Açarlar fayl sistemində saxlanmalıdır — əks halda restart onları itirir.
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("Edutor.Api");
var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

builder.Services.AddCore(builder.Configuration);
builder.Services.AddEdutorReverseProxy();
builder.Services.AddEdutorLocalization();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddSignalR();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (corsOrigins.Length > 0)
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
    else
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
}));

var authPermitLimit = builder.Configuration.GetValue<int?>("RateLimit:AuthPermitLimit") ?? 20;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "anon",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = authPermitLimit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            Edutor.Api.Common.Responses.ApiResponse.Error("Too many requests", StatusCodes.Status429TooManyRequests), ct);
    };
});

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    // Konversiya worker-ları JWT deyil, öz cihaz tokeni ilə autentifikasiya olunur.
    .AddScheme<NodeTokenAuthenticationOptions, NodeTokenAuthenticationHandler>(
        NodeTokenAuthenticationOptions.Scheme, _ => { })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Brauzer SignalR bağlantısı Authorization header göndərə bilmir — token
        // ?access_token= ilə gəlir (yalnız hub yolları üçün).
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.Student, p => p.RequireRole(AuthRoles.Student));
    options.AddPolicy(AuthPolicies.Teacher, p => p.RequireRole(AuthRoles.Teacher, AuthRoles.Admin, AuthRoles.SuperAdmin));
    options.AddPolicy(AuthPolicies.Admin, p => p.RequireRole(AuthRoles.Admin, AuthRoles.SuperAdmin));
    options.AddPolicy(AuthPolicies.SuperAdmin, p => p.RequireRole(AuthRoles.SuperAdmin));

    // Qlobal qorunma: auth olmadan heç bir endpoint işləməz.
    // Yalnız [AllowAnonymous] işarələnmiş endpoint-lər (auth: login/register/OTP) istisnadır.
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Worker tapşırıqlarının lease/reconciliation dövrü — yalnız API hostunda işləyir.
builder.Services.AddHostedService<NodesBackgroundService>();

// Abunəlik üzrə storage ölçməsi — dövri, istək zamanı deyil.
builder.Services.AddHostedService<Edutor.Api.Modules.Subscription.Services.SubscriptionMaintenanceService>();

Edutor.Core.Common.Configuration.SecretGuard.EnsureProductionSecrets(builder.Configuration, builder.Environment.IsDevelopment());

var app = builder.Build();

await app.Services.MigrateAndSeedAsync(app.Configuration);

app.UseEdutorReverseProxy(app.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();

app.UseEdutorLocalization();

// rate limiting yalnız Development xaricində (test/dev rahatlığı üçün)
if (!app.Environment.IsDevelopment())
{
    app.UseRateLimiter();
}

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.HasStarted) return;

    var message = ReasonPhrases.GetReasonPhrase(response.StatusCode);
    await response.WriteAsJsonAsync(ApiResponse.Error(message, response.StatusCode));
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

// TLS-i nginx həll edir — proxy arxasında yönləndirmə heç vaxt işləmir, yalnız xəbərdarlıq yazır.
if (!app.Configuration.GetValue<bool>("ReverseProxy:Enabled")) app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseCoreStorage();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<PaymentAccessMiddleware>();
app.MapEdutorHealth();
app.MapControllers();
app.MapHub<Edutor.Api.Modules.Feedback.Hubs.FeedbackHub>("/hubs/feedback");

app.Run();

public partial class Program { }
