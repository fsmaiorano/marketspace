using BuildingBlocks.Message.Abstractions;

namespace Order.Api.Application.Events;

public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    int PaymentMethod) : IntegrationEvent;
