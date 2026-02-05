using BuildingBlocks.BackgroundServices;
using BuildingBlocks.Loggers;
using Payment.Api.Domain.Entities;
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
        IPaymentRepository repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        try
        {
            logger.LogInformation("PaymentProcessingBackgroundService tick");

            IEnumerable<PaymentEntity> createdPayments = await repository.GetAllCreatedPaymentsAsync(isTrackingEnabled: true, stoppingToken);
            foreach (PaymentEntity payment in createdPayments)
            {
                logger.LogInformation($"Processing payment with ID: {payment.Id}");
                payment.MarkProcessing();
                await repository.PatchStatusAsync(payment, stoppingToken);
                await Task.Delay(1000, stoppingToken); // Simulate processing time
            }
        }
        catch
        {
            logger.LogError("Error retrieving repository");
        }

        await Task.CompletedTask;
    }
}