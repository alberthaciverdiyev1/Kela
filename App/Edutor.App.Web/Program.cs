using Edutor.App.Shared;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;
using Edutor.App.Shared.Services.Localization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

AppConfig.Load(builder.Configuration);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(AppConfig.ApiBaseUrl) });
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AppApi>();
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<AudioPlayerService>();
builder.Services.AddScoped<IScreenGuard, WebScreenGuard>();
builder.Services.AddScoped<IDeviceTokenProvider, WebDeviceTokenProvider>();
builder.Services.AddScoped<PushTokenService>();
builder.Services.AddScoped<Localizer>();
builder.Services.AddScoped<NavIntent>();
builder.Services.AddScoped<RefreshSignal>();

await builder.Build().RunAsync();
