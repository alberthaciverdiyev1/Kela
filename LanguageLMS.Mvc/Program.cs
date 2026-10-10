using LanguageLMS.Core;
using LanguageLMS.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
});
builder.Services.AddCore(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/home/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCoreStorage();

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/", () => Results.Redirect("/dashboard"));

app.MapControllers();

app.Run();