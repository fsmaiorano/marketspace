using BackendForFrontend.Api.Order.Services;
using BackendForFrontend.Api.Order.UseCases;

namespace BackendForFrontend.Api.Order;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services)
    {
        services.AddScoped<OrderUseCase>();
        services.AddHttpClient<OrderService>();
        return services;
    }
}
