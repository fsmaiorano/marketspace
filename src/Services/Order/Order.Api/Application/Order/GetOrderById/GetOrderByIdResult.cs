using System.Text.Json.Serialization;
using Order.Api.Application.Dto;

namespace Order.Api.Application.Order.GetOrderById;

public class GetOrderByIdResult
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public AddressDto ShippingAddress { get; init; } = null!;
    public AddressDto BillingAddress { get; init; } = null!;
    public PaymentSummaryDto Payment { get; init; } = null!;
    public IReadOnlyList<OrderItemDto> Items { get; init; } = Array.Empty<OrderItemDto>();
}