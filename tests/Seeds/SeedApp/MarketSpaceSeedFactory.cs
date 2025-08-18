using Catalog.Api.Infrastructure.Data;
using MongoDB.Driver;
using BuildingBlocks.Storage.Minio;
using Minio;

namespace SeedApp;

public class MarketSpaceSeedFactory
{
    private const string MerchantDbConnectionString =
        "Server=localhost;Port=5436;Database=MerchantDb;User Id=postgres;Password=postgres;Include Error Detail=true";
    
    private const string CatalogDbConnectionString =
        "Server=localhost;Port=5432;Database=CatalogDb;User Id=postgres;Password=postgres;Include Error Detail=true";
    
    private const string BasketDbConnectionString =
        "mongodb://localhost:27017";

    private const string MinioEndpoint = "localhost:9000";
    private const string MinioAccessKey = "admin";
    private const string MinioSecretKey = "admin123";

    public IServiceProvider Services { get; private set; }

    public MarketSpaceSeedFactory()
    {
        IHost host = CreateHost();
        Services = host.Services;
    }

    private IHost CreateHost()
    {
        IHostBuilder builder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<MerchantDbContext>(options =>
                    options.UseNpgsql(MerchantDbConnectionString));
                
                services.AddDbContext<CatalogDbContext>(options =>
                    options.UseNpgsql(CatalogDbConnectionString));
                
                services.AddSingleton<IMongoClient>(_ => new MongoClient(BasketDbConnectionString));

                services.AddScoped(sp =>
                {
                    IMongoClient client = sp.GetRequiredService<IMongoClient>();
                    return client.GetDatabase("BasketDb");
                });

                services.AddSingleton<IMinioClient>(_ =>
                {
                    return new MinioClient()
                        .WithEndpoint(MinioEndpoint)
                        .WithCredentials(MinioAccessKey, MinioSecretKey)
                        .Build();
                });

                services.AddScoped<IMinioBucket, MinioBucket>();

                services.AddScoped<IMerchantDbContext, MerchantDbContext>();
                services.AddScoped<ICatalogDbContext, CatalogDbContext>();
            });

        return builder.Build();
    }
}