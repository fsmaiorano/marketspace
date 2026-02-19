using BuildingBlocks;
using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using Order.Api.Application.Dto;
using Order.Api.Application.Order.CreateOrder;
using Order.Api.Domain.Enums;

namespace Order.Api.Application.Subscribers;

public class OnBasketCheckoutSubscriber(
    IAppLogger<OnBasketCheckoutSubscriber> logger,
    ICreateOrderHandler createOrderHandler)
    : IIntegrationEventHandler<BasketCheckoutIntegrationEvent>
{
    public async Task HandleAsync(BasketCheckoutIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Received basket checkout event for customer: {CustomerId}, EventId: {EventId}, CorrelationId: {CorrelationId}",
            @event.CustomerId, @event.EventId, @event.CorrelationId);

        logger.LogInformation(LogTypeEnum.Application,
            "ShippingAddress - FirstName: {FirstName}, LastName: {LastName}, Email: {Email}, AddressLine: {AddressLine}, Country: {Country}, State: {State}, ZipCode: {ZipCode}, Coordinates: {Coordinates}",
            @event.ShippingAddress?.FirstName ?? "NULL",
            @event.ShippingAddress?.LastName ?? "NULL",
            @event.ShippingAddress?.EmailAddress ?? "NULL",
            @event.ShippingAddress?.AddressLine ?? "NULL",
            @event.ShippingAddress?.Country ?? "NULL",
            @event.ShippingAddress?.State ?? "NULL",
            @event.ShippingAddress?.ZipCode ?? "NULL",
            @event.ShippingAddress?.Coordinates ?? "NULL");

        logger.LogInformation(LogTypeEnum.Application,
            "BillingAddress - FirstName: {FirstName}, LastName: {LastName}, Email: {Email}, AddressLine: {AddressLine}, Country: {Country}, State: {State}, ZipCode: {ZipCode}, Coordinates: {Coordinates}",
            @event.BillingAddress?.FirstName ?? "NULL",
            @event.BillingAddress?.LastName ?? "NULL",
            @event.BillingAddress?.EmailAddress ?? "NULL",
            @event.BillingAddress?.AddressLine ?? "NULL",
            @event.BillingAddress?.Country ?? "NULL",
            @event.BillingAddress?.State ?? "NULL",
            @event.BillingAddress?.ZipCode ?? "NULL",
            @event.BillingAddress?.Coordinates ?? "NULL");

        if (@event.ShippingAddress == null || @event.BillingAddress == null || @event.Payment == null)
        {
            logger.LogError(LogTypeEnum.Application, null,
                "Cannot create order: missing required data. ShippingAddress: {HasShipping}, BillingAddress: {HasBilling}, Payment: {HasPayment}",
                @event.ShippingAddress != null, @event.BillingAddress != null, @event.Payment != null);
            return;
        }

        CreateOrderCommand command = new()
        {
            CustomerId = @event.CustomerId,
            ShippingAddress = new AddressDto
            {
                FirstName = @event.ShippingAddress.FirstName,
                LastName = @event.ShippingAddress.LastName,
                EmailAddress = @event.ShippingAddress.EmailAddress,
                AddressLine = @event.ShippingAddress.AddressLine,
                Country = @event.ShippingAddress.Country,
                State = @event.ShippingAddress.State,
                ZipCode = @event.ShippingAddress.ZipCode,
                Coordinates =  @event.ShippingAddress.Coordinates
            },
            BillingAddress = new AddressDto
            {
                FirstName = @event.BillingAddress.FirstName,
                LastName = @event.BillingAddress.LastName,
                EmailAddress = @event.BillingAddress.EmailAddress,
                AddressLine = @event.BillingAddress.AddressLine,
                Country = @event.BillingAddress.Country,
                State = @event.BillingAddress.State,
                ZipCode = @event.BillingAddress.ZipCode,
                Coordinates =  @event.BillingAddress.Coordinates
            },
            Payment = new PaymentDto
            {
                CardName = @event.Payment.CardName,
                CardNumber = @event.Payment.CardNumber,
                Expiration = @event.Payment.Expiration,
                Cvv = @event.Payment.Cvv,
                PaymentMethod = @event.Payment.PaymentMethod
            },
            Status = OrderStatusEnum.Created,
            Items = [.. @event.Items.Select(item => new OrderItemDto
            {
                CatalogId = item.CatalogId,
                Quantity = item.Quantity,
                Price = item.Price
            })],
            TotalAmount = @event.TotalPrice,
            CorrelationId = @event.CorrelationId
        };

        logger.LogInformation(LogTypeEnum.Application,
            "Creating order for customer: {CustomerId}, TotalAmount: {TotalAmount}, ItemCount: {ItemCount}, CorrelationId: {CorrelationId}",
            command.CustomerId, command.TotalAmount, command.Items.Count, command.CorrelationId);

        Result<CreateOrderResult> result = await createOrderHandler.HandleAsync(command);

        if (result.IsSuccess)
        {
            logger.LogInformation(LogTypeEnum.Business,
                "Order created successfully. OrderId: {OrderId}, CustomerId: {CustomerId}",
                result.Data?.OrderId, command.CustomerId);
        }
        else
        {
            logger.LogError(LogTypeEnum.Business, null,
                "Failed to create order for customer: {CustomerId}. Error: {Error}",
                command.CustomerId, result.Error);
        }
    }
}

