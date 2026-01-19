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

    private readonly string _mongoConnectionString;
    private readonly string _minioEndpoint;
    private readonly string _minioAccessKey;
    private readonly string _minioSecretKey;

    public IServiceProvider Services { get; private set; }

    public MarketSpaceSeedFactory(
        string mongoConnectionString = BasketDbConnectionString,
        string minioEndpoint = MinioEndpoint,
        string minioAccessKey = MinioAccessKey,
        string minioSecretKey = MinioSecretKey)
    {
        _mongoConnectionString = mongoConnectionString;
        _minioEndpoint = minioEndpoint;
        _minioAccessKey = minioAccessKey;
        _minioSecretKey = minioSecretKey;
        
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
                
                services.AddSingleton<IMongoClient>(_ => new MongoClient(_mongoConnectionString));

                services.AddScoped(sp =>
                {
                    IMongoClient client = sp.GetRequiredService<IMongoClient>();
                    return client.GetDatabase("BasketDb");
                });

                services.AddSingleton<IMinioClient>(_ =>
                {
                    return new MinioClient()
                        .WithEndpoint(_minioEndpoint)
                        .WithCredentials(_minioAccessKey, _minioSecretKey)
                        .Build();
                });

                services.AddScoped<IMinioBucket, MinioBucket>();

                services.AddScoped<IMerchantDbContext, MerchantDbContext>();
                services.AddScoped<ICatalogDbContext, CatalogDbContext>();
            });

        return builder.Build();
    }
}