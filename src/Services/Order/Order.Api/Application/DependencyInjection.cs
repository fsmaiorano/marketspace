using Order.Api.Domain.Repositories;
using Order.Api.Infrastructure.Data.Repositories;

namespace Order.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        return services;
    }
}