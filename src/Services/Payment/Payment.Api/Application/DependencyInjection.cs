using BuildingBlocks.Messaging;
using Payment.Api.Application.HostedService;

namespace Payment.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        string rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
                                          ?? throw new InvalidOperationException(
                                              "RabbitMQ:ConnectionString is not configured");

        services.AddSingleton<IEventBus>(sp =>
        {
            ILogger<EventBus> logger = sp.GetRequiredService<ILogger<EventBus>>();
            return new EventBus(sp, logger, rabbitMqConnectionString);
        });

        services.AddHostedService<IntegrationEventSubscriptionService>();

        return services;
    }
}