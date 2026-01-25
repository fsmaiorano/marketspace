using Azure.Messaging.ServiceBus;
using BuildingBlocks.Message.Abstractions;
using BuildingBlocks.Message.AzureServiceBus;
using BuildingBlocks.Message.Configuration;
using BuildingBlocks.Message.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Message.DependencyInjection;

public static class ServiceCollectionExtensions
{
     public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        EventBusOptions options = new();
        IConfigurationSection section = configuration.GetSection(EventBusOptions.SectionName);
        section.Bind(options);

        options.ServiceBus.ConnectionString ??= configuration.GetConnectionString("ServiceBus")
                                                    ?? configuration.GetConnectionString("sbemulatorns")
                                                    ?? configuration["ServiceBus:ConnectionString"]
                                                    ?? configuration["AzureServiceBus:ConnectionString"];

        if (string.IsNullOrWhiteSpace(options.ServiceBus.ConnectionString))
            throw new InvalidOperationException("Service Bus connection string is not configured. Set it under MessageBroker:ServiceBus:ConnectionString or as a connection string named 'ServiceBus'.");

        services.AddSingleton<IOptions<EventBusOptions>>(_ => Options.Create(options));
        services.AddSingleton<IEventSerializer, SystemTextJsonEventSerializer>();

        if (options.Provider != EventBusProvider.AzureServiceBus)
            throw new NotSupportedException($"Event bus provider '{options.Provider}' is not supported.");

        services.AddSingleton(_ => new ServiceBusClient(options.ServiceBus.ConnectionString));
        services.AddSingleton<IEventBus, ServiceBusEventBus>();

        return services;
    }

    public static IServiceCollection AddEventHandler<THandler>(this IServiceCollection services) where THandler : class
    {
        services.AddScoped<THandler>();
        return services;
    }
}
