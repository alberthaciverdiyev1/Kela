using System.Net.Security;
using Kela.Web.Filters;
using Kela.Web.Helpers;
using Kela.Web.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options => options.Filters.Add<SessionExpiredFilter>());
builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery(o => o.HeaderName = "X-CSRF-Token");

builder.Services.AddTransient<RelayCookieHandler>();

var httpClient = builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7047");
}).AddHttpMessageHandler<RelayCookieHandler>();

if (builder.Environment.IsDevelopment())
{
    httpClient.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        SslOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
        },
    });
}

builder.Services.AddScoped<SiteConfigService>();
builder.Services.AddScoped<Localizer>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = AppConstants.WebAuthCookie;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Path = "/";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.LoginPath = "/auth/login";
        options.AccessDeniedPath = "/blocked";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    ctx.Response.Redirect(ctx.RedirectUri);
                }

                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                }
                else
                {
                    ctx.Response.Redirect(ctx.RedirectUri);
                }

                return Task.CompletedTask;
            },
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/home/error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id:int?}");


app.Run();
