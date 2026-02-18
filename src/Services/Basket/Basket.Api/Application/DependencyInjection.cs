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
        services.AddScoped<IBasketDataRepository, BasketDataRepository>();
        services.AddScoped<ICreateBasketHandler, CreateBasketHandler>();
        services.AddScoped<IDeleteBasketHandler, DeleteBasketHandler>();
        services.AddScoped<IGetBasketByIdHandler, GetBasketByIdHandler>();
        services.AddScoped<ICheckoutBasketHandler, CheckoutBasketHandler>();
        
        services.AddEventBus(configuration);

        services.AddScoped<IDomainEventHandler<BasketCheckoutDomainEvent>, OnBasketCheckoutEventHandler>();

        return services;
    }
}