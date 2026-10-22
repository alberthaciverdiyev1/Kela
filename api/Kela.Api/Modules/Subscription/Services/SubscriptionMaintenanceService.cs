using Kela.Core.Modules.Subscription;
using Kela.Core.Modules.Subscription.Abstractions;
using Microsoft.Extensions.Options;

namespace Kela.Api.Modules.Subscription.Services;

/// <summary>
/// Dövri storage ölçməsi: hər tenant-ın bucket-da tutduğu yeri hesablayıb
/// abunəlik istifadəsinə yazır. Ölçmə pahalı olduğu üçün istək zamanı deyil,
/// saatda bir neçə dəfə aparılır.
/// </summary>
public sealed class SubscriptionMaintenanceService(
    IServiceScopeFactory scopes,
    ILogger<SubscriptionMaintenanceService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // API işə düşəndə bütün tenant-ları dərhal taramaq yükü artırır.
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeSpan.FromHours(24);

            try
            {
                using var scope = scopes.CreateScope();
                var options = scope.ServiceProvider.GetRequiredService<IOptions<SubscriptionOptions>>().Value;

                if (!options.BackgroundEnabled || options.MeteringHours <= 0) return;

                delay = TimeSpan.FromHours(Math.Max(1, options.MeteringHours));

                var metering = scope.ServiceProvider.GetRequiredService<IStorageMeteringService>();
                await metering.MeterAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Abunəlik storage ölçməsi alınmadı");
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
