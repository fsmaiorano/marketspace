using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Order.Api.Domain.Events;
using Order.Api.Domain.Repositories;

namespace Order.Api.Application.EventHandlers;

public class OnOrderCreatedEventHandler(
    IAppLogger<OnOrderCreatedEventHandler> logger,
    IEventBus eventBus,
    IOrderRepository orderRepository): IDomainEventHandler<OrderCreatedDomainEvent>
{
    public Task HandleAsync(OrderCreatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}