using LanguageLMS.App.Shared;
using LanguageLMS.App.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(AppConfig.ApiBaseUrl) });
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AppApi>();
builder.Services.AddScoped<AuthState>();

await builder.Build().RunAsync();
