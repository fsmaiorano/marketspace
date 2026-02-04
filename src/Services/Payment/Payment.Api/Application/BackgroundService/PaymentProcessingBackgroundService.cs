using BuildingBlocks.BackgroundServices;
using BuildingBlocks.Loggers;
using Payment.Api.Domain.Repositories;

namespace Payment.Api.Application.BackgroundService;

public class PaymentProcessingBackgroundService(
    IAppLogger<PaymentProcessingBackgroundService> logger,
    IServiceProvider serviceProvider)
    : PeriodicBackgroundService<PaymentProcessingBackgroundService>(TimeSpan.FromSeconds(30), logger)
{
    protected override async Task ExecuteOnceAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IPaymentRepository repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        try
        {
            logger.LogInformation("PaymentProcessingBackgroundService tick");
            logger.LogDebug("Repository type: {RepoType}", repo?.GetType().FullName ?? "null");
        }
        catch
        {
            logger.LogError("Error retrieving repository");
        }

        await Task.CompletedTask;
    }
}