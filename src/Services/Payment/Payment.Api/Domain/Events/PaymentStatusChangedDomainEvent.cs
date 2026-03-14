using System.Text.Json.Serialization;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Enums;

namespace Payment.Api.Domain.Events;

public class PaymentStatusChangedDomainEvent : IDomainEvent
{
    public PaymentStatusChangedDomainEvent(PaymentEntity payment, string? correlationId = null)
    {
        OrderId = payment.OrderId;
        Status = payment.Status;
        Items = payment.Items;
        CorrelationId = correlationId;
        OccurredAt = DateTime.UtcNow;
    }

    [JsonConstructor]
    public PaymentStatusChangedDomainEvent(
        Guid orderId, PaymentStatusEnum status, List<OrderItemData> items,
        string? correlationId, DateTime occurredAt)
    {
        OrderId = orderId;
        Status = status;
        Items = items;
        CorrelationId = correlationId;
        OccurredAt = occurredAt;
    }

    public Guid OrderId { get; }
    public PaymentStatusEnum Status { get; }
    public List<OrderItemData> Items { get; }
    public string? CorrelationId { get; }
    public DateTime OccurredAt { get; }
}