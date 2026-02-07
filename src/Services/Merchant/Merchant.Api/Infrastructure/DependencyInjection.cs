using BuildingBlocks.Messaging.DomainEvents;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Messaging.Idempotency;
using Merchant.Api.Infrastructure.Data;
using Merchant.Api.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Merchant.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Try Aspire naming first (merchantdb), then fallback to default (Database)
        string connectionString = configuration.GetConnectionString("merchantdb")
                                  ?? configuration.GetConnectionString("Database")
                                  ?? throw new InvalidOperationException(
                                      "Database connection string is not configured.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddOutbox<MerchantDbContext>();
        services.AddIdempotency<MerchantDbContext>();

        services.AddDbContext<MerchantDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetRequiredService<ISaveChangesInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>());
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IMerchantDbContext, MerchantDbContext>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}