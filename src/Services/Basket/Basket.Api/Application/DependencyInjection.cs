using Basket.Api.Application.Basket.CheckoutBasket;
using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Application.Basket.DeleteBasket;
using Basket.Api.Application.Basket.GetBasketById;
using Basket.Api.Application.EventHandlers;
using Basket.Api.Domain.Events;
using Basket.Api.Domain.Repositories;
using Basket.Api.Infrastructure.Data.Repositories;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.Extensions;

namespace Basket.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<CreateBasket>();
        services.AddScoped<DeleteBasket>();
        services.AddScoped<GetBasketById>();
        services.AddScoped<CheckoutBasket>();
        
        services.AddScoped<IBasketDataRepository, BasketDataRepository>();

        services.AddEventBus(configuration);

        services.AddScoped<IDomainEventHandler<BasketCheckoutDomainEvent>, OnBasketCheckoutEventHandler>();

        return services;
    }
}