using System.Text.Json.Serialization;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;
using Order.Api.Domain.Entities;

namespace Order.Api.Domain.Events;

public class OrderCreatedDomainEvent : IDomainEvent
{
    public OrderCreatedDomainEvent(OrderEntity order, string? correlationId = null)
    {
        OrderId = order.Id.Value;
        CustomerId = order.CustomerId.Value;
        TotalAmount = order.TotalAmount.Value;
        CardNumber = order.Payment.CardNumber;
        CardName = order.Payment.CardName;
        Expiration = order.Payment.Expiration;
        Cvv = order.Payment.Cvv;
        PaymentMethod = order.Payment.PaymentMethod;
        Items = order.Items
            .Select(i => new OrderItemData
            {
                CatalogId = i.CatalogId.Value,
                Quantity = i.Quantity,
                Price = i.Price.Value
            }).ToList();
        CorrelationId = correlationId;
        OccurredAt = DateTime.UtcNow;
    }

    [JsonConstructor]
    public OrderCreatedDomainEvent(
        Guid orderId, Guid customerId, decimal totalAmount,
        string cardNumber, string cardName, string expiration, string cvv, int paymentMethod,
        List<OrderItemData> items, string? correlationId, DateTime occurredAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        CardNumber = cardNumber;
        CardName = cardName;
        Expiration = expiration;
        Cvv = cvv;
        PaymentMethod = paymentMethod;
        Items = items;
        CorrelationId = correlationId;
        OccurredAt = occurredAt;
    }

    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public string CardNumber { get; }
    public string CardName { get; }
    public string Expiration { get; }
    public string Cvv { get; }
    public int PaymentMethod { get; }
    public List<OrderItemData> Items { get; }
    public string? CorrelationId { get; }
    public DateTime OccurredAt { get; }
}
