using Basket.Api.Domain.Repositories;
using Basket.Api.Infrastructure.Data;
using Basket.Api.Infrastructure.Data.Repositories;
using Basket.Api.Infrastructure.Http.Repositories;
using BuildingBlocks.Messaging.DomainEvents;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Basket.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("basketdb")
                                  ?? configuration.GetSection("DatabaseSettings:ConnectionString").Value
                                  ?? throw new InvalidOperationException(
                                      "Database connection string is not configured.");

        NpgsqlDataSourceBuilder dataSourceBuilder = new(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        NpgsqlDataSource dataSource = dataSourceBuilder.Build();

        services.AddDbContext<BasketDbContext>(options =>
        {
            options.UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(BasketDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IBasketDbContext>(provider => provider.GetRequiredService<BasketDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IBasketDataRepository, BasketDataRepository>();
        services.AddScoped<ICheckoutHttpRepository, CheckoutHttpRepository>();

        return services;
    }
}