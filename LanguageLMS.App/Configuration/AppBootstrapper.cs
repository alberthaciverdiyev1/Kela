using LanguageLMS.App.Infrastructure.Api.Auth;
using LanguageLMS.App.Infrastructure.Api.Lessons;
using LanguageLMS.App.Infrastructure.Http;
using LanguageLMS.App.Navigation;
using LanguageLMS.App.UI.ViewModels;
using LanguageLMS.App.UI.Views.Dashboard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LanguageLMS.App.Configuration;

public static class AppBootstrapper
{
    public static IServiceProvider BuildServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables("LANGUAGELMS_")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName));

        services.AddSingleton<IAccessTokenStore, InMemoryAccessTokenStore>();
        services.AddTransient<AuthHeaderHandler>();

        services.AddHttpClient<IApiClient, ApiClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<ApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddSingleton<IAuthApi, AuthApi>();
        services.AddSingleton<ILessonApi, LessonApi>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardWindow>();

        return services.BuildServiceProvider(validateScopes: true);
    }
}
