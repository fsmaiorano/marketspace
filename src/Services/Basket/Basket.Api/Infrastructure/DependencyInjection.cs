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

        // Try Aspire naming first (basketdb), then fallback to DatabaseSettings:ConnectionString
        string connectionString = configuration.GetConnectionString("basketdb")
                                  ?? configuration.GetSection("DatabaseSettings:ConnectionString").Value
                                  ?? throw new InvalidOperationException(
                                      "Database connection string is not configured.");

        // For Aspire, extract database name from connection string if present
        // MongoDB connection string format: mongodb://host:port/database
        string databaseName = configuration.GetSection("DatabaseSettings:DatabaseName").Value ?? "BasketDb";
        
        // If using Aspire connection string, try to extract database name from URL
        if (configuration.GetConnectionString("basketdb") != null)
        {
            try
            {
                var mongoUrl = MongoUrl.Create(connectionString);
                if (!string.IsNullOrEmpty(mongoUrl.DatabaseName))
                {
                    databaseName = mongoUrl.DatabaseName;
                }
            }
            catch
            {
                // If parsing fails, use the fallback database name
            }
        }

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