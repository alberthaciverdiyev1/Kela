using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kela.Core.Common.Jobs;

/// <summary>
/// Sadə arxa plan növbəsi: HTTP sorğusu gözləməsin deyə uzun işlər (məs. yüzlərlə
/// şagirdin yaradılması) buraya atılır.
/// </summary>
public interface IBackgroundJobs
{
    void Enqueue(string name, Func<IServiceProvider, CancellationToken, Task> work);
}

public sealed class BackgroundJobs : IBackgroundJobs
{
    private readonly Channel<(string Name, Func<IServiceProvider, CancellationToken, Task> Work)> _queue =
        Channel.CreateUnbounded<(string, Func<IServiceProvider, CancellationToken, Task>)>();

    public void Enqueue(string name, Func<IServiceProvider, CancellationToken, Task> work) =>
        _queue.Writer.TryWrite((name, work));

    public IAsyncEnumerable<(string Name, Func<IServiceProvider, CancellationToken, Task> Work)> ReadAllAsync(
        CancellationToken ct) => _queue.Reader.ReadAllAsync(ct);
}

public sealed class BackgroundJobWorker(
    BackgroundJobs jobs,
    IServiceProvider services,
    ILogger<BackgroundJobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (name, work) in jobs.ReadAllAsync(stoppingToken))
        {
            using var scope = services.CreateScope();
            try
            {
                await work(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                // Bir işin uğursuzluğu növbəni dayandırmamalıdır.
                logger.LogError(ex, "Arxa plan işi uğursuz oldu: {Job}", name);
            }
        }
    }
}
