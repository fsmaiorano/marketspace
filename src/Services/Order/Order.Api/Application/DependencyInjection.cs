using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.Extensions;
using Order.Api.Application.EventHandlers;
using Order.Api.Application.HostedService;
using Order.Api.Application.Order.CreateOrder;
using Order.Api.Application.Order.DeleteOrder;
using Order.Api.Application.Order.GetOrderById;
using Order.Api.Application.Order.PatchOrderStatus;
using Order.Api.Application.Order.UpdateOrder;
using Order.Api.Application.Subscribers;
using Order.Api.Domain.Events;
using Order.Api.Domain.Repositories;
using Order.Api.Infrastructure.Data.Repositories;

namespace Order.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<CreateOrder>();
        services.AddScoped<UpdateOrder>();
        services.AddScoped<PatchOrderStatus>();
        services.AddScoped<DeleteOrder>();
        services.AddScoped<GetOrderById>();

        services.AddEventBus(configuration);

        services.AddScoped<OnBasketCheckoutSubscriber>();
        services.AddScoped<OnPaymentStatusChangedSubscriber>();
        services.AddHostedService<IntegrationEventSubscriptionService>();
        services.AddScoped<IDomainEventHandler<OrderCreatedDomainEvent>, OnOrderCreatedEventHandler>();

        return services;
    }
}