using Basket.Api.Domain.Events;
using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.Interfaces;

namespace Basket.Api.Application.EventHandlers;

public class OnBasketCheckoutEventHandler(IAppLogger<OnBasketCheckoutEventHandler> logger, IEventBus eventBus)
    : IDomainEventHandler<BasketCheckoutDomainEvent>
{
    public async Task HandleAsync(BasketCheckoutDomainEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Basket checkout domain event received for user: {UserName}, CorrelationId: {CorrelationId}",
            @event.CheckoutData.UserName, @event.CheckoutData.CorrelationId);

        BasketCheckoutIntegrationEvent integrationEvent = new()
        {
            CorrelationId = @event.CheckoutData.CorrelationId,
            CustomerId = @event.CheckoutData.CustomerId,
            UserName = @event.CheckoutData.UserName,
            ShippingAddress = new AddressData
            {
                FirstName = @event.CheckoutData.Address?.FirstName ?? string.Empty,
                LastName = @event.CheckoutData.Address?.LastName ?? string.Empty,
                EmailAddress = @event.CheckoutData.Address?.EmailAddress ?? string.Empty,
                AddressLine = @event.CheckoutData.Address?.AddressLine ?? string.Empty,
                Country = @event.CheckoutData.Address?.Country ?? string.Empty,
                State = @event.CheckoutData.Address?.State ?? string.Empty,
                ZipCode = @event.CheckoutData.Address?.ZipCode ?? string.Empty,
                Coordinates =  @event.CheckoutData.Address?.Coordinates ?? string.Empty,
            },
            BillingAddress = new AddressData
            {
                FirstName = @event.CheckoutData.Address?.FirstName ?? string.Empty,
                LastName = @event.CheckoutData.Address?.LastName ?? string.Empty,
                EmailAddress = @event.CheckoutData.Address?.EmailAddress ?? string.Empty,
                AddressLine = @event.CheckoutData.Address?.AddressLine ?? string.Empty,
                Country = @event.CheckoutData.Address?.Country ?? string.Empty,
                State = @event.CheckoutData.Address?.State ?? string.Empty,
                ZipCode = @event.CheckoutData.Address?.ZipCode ?? string.Empty,
                Coordinates =  @event.CheckoutData.Address?.Coordinates ?? string.Empty,
            },
            Payment = new PaymentData
            {
                CardName = @event.CheckoutData.Payment?.CardName ?? string.Empty,
                CardNumber = @event.CheckoutData.Payment?.CardNumber ?? string.Empty,
                Expiration = @event.CheckoutData.Payment?.Expiration ?? string.Empty,
                Cvv = @event.CheckoutData.Payment?.Cvv ?? string.Empty,
                PaymentMethod = @event.CheckoutData.Payment?.PaymentMethod ?? 0
            },
            Items = [.. @event.ShoppingCart.Items.Select(item => new OrderItemData
            {
                CatalogId = Guid.Parse(item.ProductId),
                Quantity = item.Quantity,
                Price = item.Price
            })],
            TotalPrice = @event.ShoppingCart.TotalPrice
        };

        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(LogTypeEnum.Application,
            "Basket checkout integration event published for customer: {CustomerId}, TotalPrice: {TotalPrice}, ItemCount: {ItemCount}, CorrelationId: {CorrelationId}",
            @event.CheckoutData.CustomerId, @event.ShoppingCart.TotalPrice, @event.ShoppingCart.Items.Count, @event.CheckoutData.CorrelationId);
    }
}
