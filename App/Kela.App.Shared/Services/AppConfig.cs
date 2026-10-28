using Microsoft.Extensions.Configuration;

namespace Kela.App.Shared.Services;

public static class AppConfig
{
    /// <summary>API ünvanı — host-un appsettings.json-undan (`Api:BaseUrl`) yüklənir.</summary>
    public static string ApiBaseUrl { get; private set; } = string.Empty;

    public static bool ShowWatermark { get; set; } = true;

    public static void Load(IConfiguration configuration)
    {
        var value = configuration["Api:BaseUrl"]?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Api:BaseUrl appsettings.json-da təyin edilməyib.");
        }

        ApiBaseUrl = value;
    }
}
