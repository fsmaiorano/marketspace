using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;

namespace BuildingBlocks.Messaging.IntegrationEvents;

public class OrderCreatedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CardNumber { get; init; } = null!;
    public string CardName { get; init; } = null!;
    public string Expiration { get; init; } = null!;
    public string Cvv { get; init; } = null!;
    public int PaymentMethod { get; set; } = 0!;
    public decimal TotalAmount { get; init; }
}