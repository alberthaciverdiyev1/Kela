using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Core.Modules.Nodes;
using Microsoft.Extensions.Options;

namespace Kela.Api.Modules.Nodes.Services;

/// <summary>
/// Fon xidməti: lease-i bitmiş tapşırıqları geri qaytarır, cihazları offline işarələyir,
/// işsiz qalmış videolara tapşırıq yaradır və bütün pillələri bitmiş videoların
/// master playlist-ini yazır. Bir neçə API nüsxəsi paralel işləsə də nəticə eynidir.
/// </summary>
public sealed class NodesBackgroundService(
    IServiceScopeFactory scopes,
    ILogger<NodesBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeSpan.FromSeconds(60);

            try
            {
                using var scope = scopes.CreateScope();
                var scheduler = scope.ServiceProvider.GetRequiredService<IVideoJobScheduler>();
                var lifecycle = scope.ServiceProvider.GetRequiredService<IVideoJobLifecycle>();
                var options = scope.ServiceProvider.GetRequiredService<IOptions<NodeOptions>>().Value;

                if (!options.BackgroundEnabled) return;

                delay = TimeSpan.FromSeconds(Math.Max(10, options.ReconcileSeconds));

                await scheduler.ReapAsync(stoppingToken);

                foreach (var videoId in await scheduler.ReconcileAsync(stoppingToken))
                {
                    await lifecycle.FinalizeAsync(videoId, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Nodes reconciliation pass failed");
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
