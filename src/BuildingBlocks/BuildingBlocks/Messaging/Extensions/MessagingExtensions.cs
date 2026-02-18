using BuildingBlocks.Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Extensions;

public static class MessagingExtensions
{
    /// <summary>
    /// Adds the EventBus service to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        string rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
                                          ?? throw new InvalidOperationException("RabbitMQ:ConnectionString is not configured");

        services.AddSingleton<IEventBus>(sp =>
        {
            ILogger<EventBus> logger = sp.GetRequiredService<ILogger<EventBus>>();
            return new EventBus(sp, logger, rabbitMqConnectionString);
        });

        return services;
    }
}
