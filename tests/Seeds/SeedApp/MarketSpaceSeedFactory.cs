using Basket.Api.Infrastructure.Data;
using Catalog.Api.Infrastructure.Data;
using BuildingBlocks.Storage.Minio;
using Minio;
using Npgsql;

namespace SeedApp;

public class MarketSpaceSeedFactory
{
    private const string MerchantDbConnectionString =
        "Server=localhost;Port=5436;Database=MerchantDb;User.Api Id=postgres;Password=postgres;Include Error Detail=true";

    private const string CatalogDbConnectionString =
        "Server=localhost;Port=5432;Database=CatalogDb;User.Api Id=postgres;Password=postgres;Include Error Detail=true";

    private const string BasketDbConnectionString =
        "Server=localhost;Port=5433;Database=BasketDb;User.Api Id=postgres;Password=postgres;Include Error Detail=true";

    private const string MinioEndpoint = "localhost:9000";
    private const string MinioAccessKey = "admin";
    private const string MinioSecretKey = "admin123";

    private readonly string _merchantConnectionString;
    private readonly string _catalogConnectionString;
    private readonly string _basketConnectionString;
    private readonly string _minioEndpoint;
    private readonly string _minioAccessKey;
    private readonly string _minioSecretKey;

    public IServiceProvider Services { get; private set; }

    public MarketSpaceSeedFactory(
        string merchantConnectionString = MerchantDbConnectionString,
        string catalogConnectionString = CatalogDbConnectionString,
        string basketConnectionString = BasketDbConnectionString,
        string minioEndpoint = MinioEndpoint,
        string minioAccessKey = MinioAccessKey,
        string minioSecretKey = MinioSecretKey)
    {
        _merchantConnectionString = merchantConnectionString;
        _catalogConnectionString = catalogConnectionString;
        _basketConnectionString = basketConnectionString;
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
                    options.UseNpgsql(_merchantConnectionString));

                services.AddDbContext<CatalogDbContext>(options =>
                    options.UseNpgsql(_catalogConnectionString));

                NpgsqlDataSourceBuilder basketDataSourceBuilder = new(_basketConnectionString);
                basketDataSourceBuilder.EnableDynamicJson();
                NpgsqlDataSource basketDataSource = basketDataSourceBuilder.Build();

                services.AddDbContext<BasketDbContext>(options =>
                    options.UseNpgsql(basketDataSource));

                services.AddSingleton<IMinioClient>(_ => new MinioClient()
                    .WithEndpoint(_minioEndpoint)
                    .WithCredentials(_minioAccessKey, _minioSecretKey)
                    .Build());

                services.AddScoped<IMinioBucket, MinioBucket>();

                services.AddScoped<IMerchantDbContext, MerchantDbContext>();
                services.AddScoped<ICatalogDbContext, CatalogDbContext>();
                services.AddScoped<IBasketDbContext, BasketDbContext>();
            });

        return builder.Build();
    }
}