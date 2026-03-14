using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;
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
            "Payment status changed event received. CorrelationId: {CorrelationId}, Status: {Status}",
            @event.CorrelationId, @event.Status);

        PaymentStatusChangedIntegrationEvent integrationEvent = new()
        {
            CorrelationId = @event.CorrelationId,
            OrderId = @event.OrderId,
            PaymentStatus = (int)@event.Status,
            PaymentStatusName = @event.Status.ToString(),
            Items = @event.Items
        };

        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(LogTypeEnum.Application,
            "Payment status changed integration event published. CorrelationId: {CorrelationId}, PaymentStatus: {PaymentStatus}",
            @event.CorrelationId, integrationEvent.PaymentStatus);
    }
}