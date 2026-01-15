using Basket.Api.Domain.Repositories;
using Basket.Api.Infrastructure.Data;
using Basket.Api.Infrastructure.Data.Repositories;
using Basket.Api.Infrastructure.Http.Repositories;
using MongoDB.Driver;

namespace Basket.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseSettings>(
            configuration.GetSection("DatabaseSettings"));

        string connectionString = configuration.GetSection("DatabaseSettings:ConnectionString").Value
                                  ?? throw new InvalidOperationException(
                                      "Database connection string is not configured.");

        string databaseName = configuration.GetSection("DatabaseSettings:DatabaseName").Value
                              ?? throw new InvalidOperationException("Database name is not configured.");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.AddScoped(sp =>
        {
            IMongoClient client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IBasketDataRepository, BasketDataRepository>();
        services.AddScoped<ICheckoutHttpRepository, CheckoutHttpRepository>();

        return services;
    }
}