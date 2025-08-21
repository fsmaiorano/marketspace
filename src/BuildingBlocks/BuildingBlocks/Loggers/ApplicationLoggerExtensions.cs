using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BuildingBlocks.Services.Correlation;

namespace BuildingBlocks.Loggers;

public static class ApplicationLoggerExtensions
{
    public static IServiceCollection AddApplicationLogger(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICorrelationIdService, CorrelationIdService>();
        services.TryAddScoped<IApplicationLogger, ApplicationLogger>();

        return services;
    }
}