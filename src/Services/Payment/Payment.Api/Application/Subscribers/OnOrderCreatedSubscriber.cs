using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace Payment.Api.Application.Subscribers;

public class OnOrderCreatedSubscriber(IAppLogger<OnOrderCreatedSubscriber> logger)
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Order created integration event received in Payment Service. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, CorrelationId: {CorrelationId}",
            @event.OrderId, @event.CustomerId, @event.TotalAmount, @event.CorrelationId);
        throw new NotImplementedException();
    }
}