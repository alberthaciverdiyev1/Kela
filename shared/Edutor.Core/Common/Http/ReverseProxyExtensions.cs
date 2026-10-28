using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Edutor.Core.Common.Http;

/// <summary>
/// TLS-i reverse proxy həll edəndə sxem/IP məlumatının itməməsi üçün.
/// Yalnız `ReverseProxy:Enabled=true` olduqda aktivdir — birbaşa açıq host-da
/// X-Forwarded-* başlıqlarına etibar etmək təhlükəlidir.
/// </summary>
public static class ReverseProxyExtensions
{
    public static IServiceCollection AddEdutorReverseProxy(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
        return services;
    }

    public static IApplicationBuilder UseEdutorReverseProxy(this IApplicationBuilder app, IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("ReverseProxy:Enabled")) return app;

        var options = app.ApplicationServices.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
        foreach (var proxy in configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
        {
            if (IPAddress.TryParse(proxy, out var ip)) options.KnownProxies.Add(ip);
        }

        app.UseForwardedHeaders(options);
        return app;
    }
}
