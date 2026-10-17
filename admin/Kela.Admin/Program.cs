using Kela.Core;
using Kela.Core.Common.Localization;
using Kela.Core.Data;
using Kela.Core.Modules.Auth;
using Kela.Admin.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
});
builder.Services.AddCore(builder.Configuration);
builder.Services.AddKelaLocalization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
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

var app = builder.Build();

await app.Services.SeedDefaultAdminAsync(app.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/home/error");
    app.UseHsts();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCoreStorage();

app.UseKelaLocalization();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/", () => Results.Redirect("/dashboard"));

app.MapControllers();

app.Run();
