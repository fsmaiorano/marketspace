using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.Interfaces;
using Payment.Api.Application.EventHandlers;
using Payment.Api.Application.HostedService;
using Payment.Api.Application.Payment.CreatePayment;
using Payment.Api.Application.Payment.DeletePayment;
using Payment.Api.Application.Payment.GetPaymentById;
using Payment.Api.Application.Payment.UpdatePayment;
using Payment.Api.Application.Subscribers;
using Payment.Api.Domain.Events;
using Payment.Api.Domain.Repositories;
using Payment.Api.Infrastructure.Data.Repositories;

namespace Payment.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICreatePaymentHandler, CreatePaymentHandler>();
        services.AddScoped<IGetPaymentByIdHandler, GetPaymentByIdHandler>();
        services.AddScoped<IUpdatePaymentHandler, UpdatePaymentHandler>();
        services.AddScoped<IDeletePaymentHandler, DeletePaymentHandler>();

        string rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
                                          ?? throw new InvalidOperationException(
                                              "RabbitMQ:ConnectionString is not configured");

        services.AddSingleton<IEventBus>(sp =>
        {
            ILogger<EventBus> logger = sp.GetRequiredService<ILogger<EventBus>>();
            return new EventBus(sp, logger, rabbitMqConnectionString);
        });

        services.AddScoped<OnOrderCreatedSubscriber>();
        services.AddHostedService<IntegrationEventSubscriptionService>();
        services.AddScoped<IDomainEventHandler<PaymentStatusChangedDomainEvent>, OnPaymentStatusChangedEventHandler>();

        return services;
    }
}