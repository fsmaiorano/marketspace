using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Order.Api.Infrastructure.Data;
using Order.Api.Infrastructure.Data.Interceptors;

namespace Order.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Try Aspire naming first (orderdb), then fallback to default (Database)
        string connectionString = configuration.GetConnectionString("orderdb")
                                  ?? configuration.GetConnectionString("Database")
                                  ?? throw new InvalidOperationException(
                                      "Database connection string is not configured.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetRequiredService<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IOrderDbContext, OrderDbContext>();

        return services;
    }
}