using BackendForFrontend.Api.Aggregations.Contracts;
using BackendForFrontend.Api.Aggregations.UseCases;

namespace BackendForFrontend.Api.Aggregations;

public static class DependencyInjection
{
    public static IServiceCollection AddAggregationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ICustomerDashboardUseCase, CustomerDashboardUseCase>();
        
        return services;
    }
}
