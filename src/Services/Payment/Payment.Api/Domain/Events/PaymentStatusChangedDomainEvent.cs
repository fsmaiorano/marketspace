using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Payment.Api.Domain.Entities;

namespace Payment.Api.Domain.Events;

public class PaymentStatusChangedDomainEvent(PaymentEntity payment, string? correlationId = null) : IDomainEvent
{
    public PaymentEntity Payment { get; } = payment;
    public string? CorrelationId { get; } = correlationId;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}