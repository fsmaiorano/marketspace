using Basket.Api.Domain.Events;
using Basket.Api.Domain.ValueObjects;
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

        AddressData address = MapAddress(@event.CheckoutData.Address);

        logger.LogDebug(
            "Publishing BasketCheckoutIntegrationEvent — {FirstName} {LastName}, {AddressLine}, {Country}",
            address.FirstName, address.LastName, address.AddressLine, address.Country);

        BasketCheckoutIntegrationEvent integrationEvent = new()
        {
            CorrelationId = @event.CheckoutData.CorrelationId,
            CustomerId = @event.CheckoutData.CustomerId,
            UserName = @event.CheckoutData.UserName,
            ShippingAddress = address,
            BillingAddress = address,
            Payment = new PaymentData
            {
                CardName = @event.CheckoutData.Payment?.CardName ?? string.Empty,
                CardNumber = @event.CheckoutData.Payment?.CardNumber ?? string.Empty,
                Expiration = @event.CheckoutData.Payment?.Expiration ?? string.Empty,
                Cvv = @event.CheckoutData.Payment?.Cvv ?? string.Empty,
                PaymentMethod = @event.CheckoutData.Payment?.PaymentMethod ?? 0
            },
            Items = @event.Items,
            TotalPrice = @event.TotalPrice
        };

        await eventBus.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(LogTypeEnum.Application,
            "Basket checkout integration event published for customer: {CustomerId}, TotalPrice: {TotalPrice}, ItemCount: {ItemCount}, CorrelationId: {CorrelationId}",
            @event.CheckoutData.CustomerId, @event.TotalPrice, @event.Items.Count, @event.CheckoutData.CorrelationId);
    }

    private static AddressData MapAddress(CheckoutAddress? source) => new()
    {
        FirstName = source?.FirstName ?? string.Empty,
        LastName = source?.LastName ?? string.Empty,
        EmailAddress = source?.EmailAddress ?? string.Empty,
        AddressLine = source?.AddressLine ?? string.Empty,
        Country = source?.Country ?? string.Empty,
        State = source?.State ?? string.Empty,
        ZipCode = source?.ZipCode ?? string.Empty,
        Coordinates = source?.Coordinates ?? string.Empty,
    };
}
