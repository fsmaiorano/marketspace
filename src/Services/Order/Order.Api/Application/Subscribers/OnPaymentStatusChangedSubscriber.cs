using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using Order.Api.Application.Order.UpdateOrder;

namespace Order.Api.Application.Subscribers;

public class OnPaymentStatusChangedSubscriber(
    IAppLogger<OnPaymentStatusChangedSubscriber> logger,
    IUpdateOrderHandler updateOrderHandler) : IIntegrationEventHandler<PaymentStatusChangedIntegrationEvent>
{
    public Task HandleAsync(PaymentStatusChangedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        //fazer um método de patch para atualizar apensar o status da order
        Guid orderId = @event.OrderId;
        throw new NotImplementedException();
    }
}