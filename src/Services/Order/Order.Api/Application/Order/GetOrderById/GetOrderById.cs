using BuildingBlocks;
using BuildingBlocks.Loggers;
using Order.Api.Application.Dto;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using System.Collections.ObjectModel;

namespace Order.Api.Application.Order.GetOrderById;

public record GetOrderByIdQuery(Guid Id); 

public record GetOrderByIdResult
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public AddressDto ShippingAddress { get; init; } = null!;
    public AddressDto BillingAddress { get; init; } = null!;
    public PaymentSummaryDto Payment { get; init; } = null!;
    public IReadOnlyList<OrderItemDto> Items { get; init; } = [];
}

public class GetOrderById(
    IOrderRepository repository,
    IAppLogger<GetOrderById> logger)
{
    public async Task<Result<GetOrderByIdResult>> HandleAsync(GetOrderByIdQuery query)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing get order by ID request for: {OrderId}", query.Id);
            OrderId orderId = OrderId.Of(query.Id);
            OrderEntity? order = await repository.GetByIdAsync(orderId, isTrackingEnabled: false);

            if (order is null)
                return Result<GetOrderByIdResult>.Failure($"Order with ID {query.Id} not found.");

            GetOrderByIdResult result = new()

            {
                Id = order.Id.Value,
                CustomerId = order.CustomerId.Value,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount.Value,
                ShippingAddress = new AddressDto
                {
                    FirstName = order.ShippingAddress.FirstName,
                    LastName = order.ShippingAddress.LastName,
                    EmailAddress = order.ShippingAddress.EmailAddress,
                    AddressLine = order.ShippingAddress.AddressLine,
                    Country = order.ShippingAddress.Country,
                    State = order.ShippingAddress.State,
                    ZipCode = order.ShippingAddress.ZipCode
                },
                BillingAddress = new AddressDto
                {
                    FirstName = order.BillingAddress.FirstName,
                    LastName = order.BillingAddress.LastName,
                    EmailAddress = order.BillingAddress.EmailAddress,
                    AddressLine = order.BillingAddress.AddressLine,
                    Country = order.BillingAddress.Country,
                    State = order.BillingAddress.State,
                    ZipCode = order.BillingAddress.ZipCode
                },
                Payment = new PaymentSummaryDto
                {
                    CardName = order.Payment.CardName,
                    MaskedCardNumber = MaskCard(order.Payment.CardNumber),
                    PaymentMethod = order.Payment.PaymentMethod
                },
                Items = order.Items
                    .Select(item => new OrderItemDto
                    {
                        OrderId = item.OrderId.Value,
                        CatalogId = item.CatalogId.Value,
                        Quantity = item.Quantity,
                        Price = item.Price.Value
                    })
                    .ToList()
            };

            return Result<GetOrderByIdResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while getting order by ID.");
            return Result<GetOrderByIdResult>.Failure("An error occurred while processing your request.");
        }
    }

    private static string MaskCard(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
            return "****";
        string last4 = cardNumber[^4..];
        return new string('*', Math.Max(0, cardNumber.Length - 4)) + last4;
    }
}