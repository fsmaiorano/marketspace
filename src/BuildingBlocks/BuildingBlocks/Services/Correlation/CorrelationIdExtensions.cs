using BuildingBlocks.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Services.Correlation;

public static class CorrelationIdExtensions
{
    /// <summary>
    /// Registers CorrelationId services and HTTP client handlers in the DI container.
    /// </summary>
    public static IServiceCollection AddCorrelationIdServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdService, CorrelationIdService>();
        services.AddScoped<CorrelationIdHandler>();
        services.AddTransient<LoggingHandler>();
        return services;
    }
}
