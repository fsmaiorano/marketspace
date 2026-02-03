using BuildingBlocks.Messaging.DomainEvents;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Payment.Api.Infrastructure.Data;
using Payment.Api.Infrastructure.Data.Interceptors;
using Payment.Api.Domain.Repositories;
using Payment.Api.Infrastructure.Data.Repositories;

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

        services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetRequiredService<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IPaymentDbContext, PaymentDbContext>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        return services;
    }
}