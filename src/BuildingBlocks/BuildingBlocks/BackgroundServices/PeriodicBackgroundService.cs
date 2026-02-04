using BuildingBlocks.Loggers;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.BackgroundServices;

public abstract class PeriodicBackgroundService<T>(TimeSpan interval, IAppLogger<T> logger) : BackgroundService
{
    protected abstract Task ExecuteOnceAsync(CancellationToken stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(interval);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error executing periodic work");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
        }
        catch (OperationCanceledException) { }
    }
}