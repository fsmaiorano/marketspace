using BuildingBlocks.Messaging.DomainEvents;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Payment.Api.Infrastructure.Data;
using Payment.Api.Infrastructure.Data.Interceptors;
using Payment.Api.Domain.Repositories;
using Payment.Api.Infrastructure.Data.Repositories;

using BuildingBlocks.Messaging.Outbox;

namespace Payment.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("paymentdb")
                                  ?? configuration.GetConnectionString("Database")
                                  ?? throw new InvalidOperationException(
                                      "Database connection string is not configured.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddOutbox<PaymentDbContext>();
        services.AddIdempotency<PaymentDbContext>();

        services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetRequiredService<ISaveChangesInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>());
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IPaymentDbContext, PaymentDbContext>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        return services;
    }
}