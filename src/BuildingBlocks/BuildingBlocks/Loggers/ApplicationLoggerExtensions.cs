using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BuildingBlocks.Services.Correlation;

namespace BuildingBlocks.Loggers;

public static class ApplicationLoggerExtensions
{
    public static IServiceCollection AddApplicationLogger(this IServiceCollection services)
    {
        services.TryAddScoped<ICorrelationIdService, CorrelationIdService>();
        services.TryAddScoped<IApplicationLogger, ApplicationLogger>();
        services.TryAddScoped<IApplicationLoggerFactory, ApplicationLoggerFactory>();

        return services;
    }
}