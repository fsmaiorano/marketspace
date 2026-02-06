using BuildingBlocks.BackgroundServices;
using BuildingBlocks.Loggers;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Repositories;

namespace Payment.Api.Application.BackgroundService;

public class PaymentProcessingBackgroundService(
    IAppLogger<PaymentProcessingBackgroundService> logger,
    IServiceProvider serviceProvider)
    : PeriodicBackgroundService<PaymentProcessingBackgroundService>(TimeSpan.FromSeconds(10), logger)
{
    protected override async Task ExecuteOnceAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IPaymentRepository repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        try
        {
            logger.LogInformation("PaymentProcessingBackgroundService tick");

            IEnumerable<PaymentEntity> createdPayments =
                await repository.GetByStatus(PaymentStatusEnum.Created, isTrackingEnabled: true, stoppingToken);
            foreach (PaymentEntity payment in createdPayments)
            {
                logger.LogInformation($"Processing payment with ID: {payment.Id}");
                payment.MarkProcessing();
                await repository.PatchStatusAsync(payment, stoppingToken);
            }

            IEnumerable<PaymentEntity> processingPayments = await repository.GetByStatus(PaymentStatusEnum.Processing,
                isTrackingEnabled: true, stoppingToken);
            foreach (PaymentEntity payment in processingPayments)
            {
                logger.LogInformation($"Processing payment with ID: {payment.Id}");
                // In the future, add here the logic to risk, analysis, fraud detection, etc.
                // For now, we just mark the payment as authorized
                payment.MarkCaptured(); 
                
                await repository.PatchStatusAsync(payment, stoppingToken);
            }
        }
        catch
        {
            logger.LogError("Error retrieving repository");
        }

        await Task.CompletedTask;
    }
}