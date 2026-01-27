using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Order.Api.Application.EventHandlers;
using Order.Api.Application.Order.CreateOrder;
using Order.Api.Application.Order.DeleteOrder;
using Order.Api.Application.Order.GetOrderById;
using Order.Api.Application.Order.UpdateOrder;
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
        services.AddScoped<ICreateOrderHandler, CreateOrderHandler>();
        services.AddScoped<IUpdateOrderHandler, UpdateOrderHandler>();
        services.AddScoped<IDeleteOrderHandler, DeleteOrderHandler>();
        services.AddScoped<IGetOrderByIdHandler, GetOrderByIdHandler>();
        
        string rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq") 
                                          ?? throw new InvalidOperationException("RabbitMQ:ConnectionString is not configured");
        
        services.AddSingleton<IEventBus>(sp =>
        {
            ILogger<EventBus> logger = sp.GetRequiredService<ILogger<EventBus>>();
            return new EventBus(sp, logger, rabbitMqConnectionString);
        });

        services.AddScoped<IDomainEventHandler<OrderCreatedDomainEvent>, OnOrderCreatedEventHandler>();

        return services;
    }
}