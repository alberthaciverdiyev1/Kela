using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;
using Edutor.App.Shared.Services.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Configuration;

namespace Edutor.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppConfig.ShowWatermark = false; // native: FLAG_SECURE MainActivity-də tətbiq olunur, watermark lazımsız

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        // API ünvanı paketdəki appsettings.json-dan gəlir (Resources/Raw → LogicalName).
        using (var settings = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult())
        {
            builder.Configuration.AddJsonStream(settings);
        }

        AppConfig.Load(builder.Configuration);

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // App.Shared ilə eyni DI (web tərəfdəki App.Web/Program.cs kimi)
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

        return builder.Build();
    }
}
