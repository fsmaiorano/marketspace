using Order.Api.Application.Order.CreateOrder;
using Order.Api.Application.Order.DeleteOrder;
using Order.Api.Application.Order.GetOrderById;
using Order.Api.Application.Order.UpdateOrder;
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

        return services;
    }
}