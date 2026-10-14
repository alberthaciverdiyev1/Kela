using Kela.App.Shared;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(AppConfig.ApiBaseUrl) });
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AppApi>();
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<AudioPlayerService>();
builder.Services.AddScoped<IScreenGuard, WebScreenGuard>();
builder.Services.AddScoped<IDeviceTokenProvider, WebDeviceTokenProvider>();
builder.Services.AddScoped<PushTokenService>();

await builder.Build().RunAsync();
