using BackendForFrontend.Api.Order.Contracts;
using BackendForFrontend.Api.Order.Services;
using BackendForFrontend.Api.Order.UseCases;

namespace BackendForFrontend.Api.Order;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IOrderUseCase, OrderUseCase>();
        
        // Register HttpClient for OrderService
        services.AddHttpClient<IOrderService, OrderService>();
        
        return services;
    }
}
