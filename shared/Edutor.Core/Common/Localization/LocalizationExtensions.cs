using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Core.Common.Localization;

public static class LocalizationExtensions
{
    /// <summary>Dəstəklənən dillər: az (default), en, ru, tr. Dil Accept-Language header-indən seçilir.</summary>
    public static readonly string[] SupportedCultures = ["az", "en", "ru", "tr"];

    public static IServiceCollection AddEdutorLocalization(this IServiceCollection services)
    {
        services.AddLocalization();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture("az");
            options.AddSupportedCultures(SupportedCultures);
            options.AddSupportedUICultures(SupportedCultures);
            // Əvvəl istifadəçinin seçimi (cookie), sonra brauzerin Accept-Language-ı.
            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
            options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
        });

        return services;
    }

    public static IApplicationBuilder UseEdutorLocalization(this IApplicationBuilder app)
        => app.UseRequestLocalization();
}
