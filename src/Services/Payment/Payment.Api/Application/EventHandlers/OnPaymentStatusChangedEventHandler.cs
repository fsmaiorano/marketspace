using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Events;

namespace Payment.Api.Application.EventHandlers;

public class OnPaymentStatusChangedEventHandler(
    IAppLogger<OnPaymentStatusChangedEventHandler> logger,
    IEventBus eventBus)
    : IDomainEventHandler<PaymentStatusChangedDomainEvent>
{
    public async Task HandleAsync(PaymentStatusChangedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Payment status changed event received. CorrelationId: {@event.CorrelationId}", @event.CorrelationId);

        PaymentStatusChangedIntegrationEvent integrationEvent = new()
        {
            CorrelationId = @event.CorrelationId,
            OrderId = @event.Payment.OrderId,
            IsProcessing = @event.Payment.Status == PaymentStatusEnum.Processing,
            IsAuthorized = @event.Payment.Status == PaymentStatusEnum.Authorized,
        };

        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(LogTypeEnum.Application,
            $"Payment status changed integration event published. CorrelationId: {@event.CorrelationId}",
            @event.CorrelationId);
    }
}