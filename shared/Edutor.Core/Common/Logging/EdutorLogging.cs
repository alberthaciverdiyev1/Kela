using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

namespace Edutor.Core.Common.Logging;

public static class EdutorLogging
{
    /// <summary>
    /// Konsola (journald) bütün səviyyələri, gündəlik fayla isə yalnız xəbərdarlıq və
    /// yuxarısını yazır. <c>Logging:File:Path</c> boşdursa fayl sink-i işə düşmür.
    /// </summary>
    public static WebApplicationBuilder UseEdutorLogging(this WebApplicationBuilder builder, string appName)
    {
        var file = builder.Configuration.GetSection("Logging:File");
        var minimum = Enum.TryParse<LogEventLevel>(file["MinimumLevel"], ignoreCase: true, out var level)
            ? level
            : LogEventLevel.Warning;

        var configuration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

        var path = file["Path"];
        if (!string.IsNullOrWhiteSpace(path))
        {
            configuration = configuration.WriteTo.File(
                Path.Combine(path, $"{appName}-.log"),
                restrictedToMinimumLevel: minimum,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
        }

        Log.Logger = configuration.CreateLogger();
        builder.Host.UseSerilog();

        return builder;
    }
}
