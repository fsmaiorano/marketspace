using BackendForFrontend.Api;
using BackendForFrontend.Api.Merchant.Contracts;
using Basket.Api.Infrastructure.Data;
using Basket.Test.Api;
using Catalog.Api.Infrastructure.Data;
using Catalog.Test.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minio;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Merchant.Api.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mongo2Go;
using MongoDB.Driver;
using Order.Api.Infrastructure.Data;
using Order.Test.Api;
using Serilog.Extensions.Hosting;
using BackendForFrontend.Api.Merchant.Services;
using Merchant.Test.Api;
using BackendForFrontend.Test.Mocks;

namespace BackendForFrontend.Test.Api;

public class BackendForFrontendFactory : WebApplicationFactory<BackendForFrontendProgram>, IAsyncLifetime
{
    private readonly IContainer _minioContainer;
    private readonly MerchantApiFactory _merchantApiFactory;
    private HttpClient _merchantApiClient;

    public string MinioEndpoint { get; private set; } = string.Empty;
    public string AccessKey => "admin";
    public string SecretKey => "admin123";

    public BackendForFrontendFactory()
    {
        _minioContainer = new ContainerBuilder()
            .WithImage("minio/minio:latest")
            .WithEnvironment("MINIO_ROOT_USER", AccessKey)
            .WithEnvironment("MINIO_ROOT_PASSWORD", SecretKey)
            .WithPortBinding(9000, true)
            .WithCommand("server", "/data", "--console-address", ":9001")
            .Build();

        _merchantApiFactory = new MerchantApiFactory();
        _merchantApiClient = _merchantApiFactory.CreateClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            List<ServiceDescriptor> descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            (d.ServiceType == typeof(BasketDbContext) ||
                             d.ServiceType == typeof(IBasketDbContext) ||
                             d.ServiceType == typeof(MerchantDbContext) ||
                             d.ServiceType == typeof(IMerchantDbContext) ||
                             d.ServiceType == typeof(OrderDbContext) ||
                             d.ServiceType == typeof(CatalogDbContext) ||
                             d.ServiceType == typeof(ICatalogDbContext) ||
                             d.ServiceType == typeof(MinioClient) ||
                             d.ServiceType == typeof(IMinioClient) ||
                             d.ServiceType == typeof(HttpClient) ||
                             d.ServiceType.FullName.Contains(nameof(OrderDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IOrderDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(MerchantDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IMerchantDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(BasketDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IBasketDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IMongoClient)) ||
                             d.ServiceType.FullName.Contains(nameof(CatalogDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(ICatalogDbContext)) ||
                             d.ServiceType.FullName.Contains("EntityFramework") ||
                             d.ServiceType.FullName.Contains("Npgsql")) ||
                            d.ServiceKey?.ToString() == nameof(MerchantService) ||
                            d.ServiceKey?.ToString() == nameof(IMerchantService)

                    // d.ServiceKey?.ToString() == nameof(IOrderService) ||
                    // d.ServiceKey?.ToString() == nameof(OrdertService) ||
                    //
                    // d.ServiceKey?.ToString() == nameof(ICatalogService) ||
                    // d.ServiceKey?.ToString() == nameof(ICatalogService) ||
                    //
                    // d.ServiceKey?.ToString() == nameof(IBasketService) ||
                    // d.ServiceKey?.ToString() == nameof(IBasketService) 
                )
                .ToList();

            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.RemoveAll<ILoggerFactory>();
            services.TryAddSingleton<DiagnosticContext>();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Warning));

            MongoDbRunner? runner = MongoDbRunner.Start();

            services.AddSingleton<IMongoClient>(sp => new MongoClient(runner.ConnectionString));

            services.AddScoped(sp =>
            {
                IMongoClient client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase("BasketInMemoryDbForTesting");
            });

            services.AddDbContext<MerchantDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.AddScoped<BasketApiFactory>();
            services.AddScoped<OrderApiFactory>();
            services.AddScoped<CatalogApiFactory>();

            services.AddScoped<IMerchantService>(provider =>
            {
                ILogger<TestMerchantService> logger = provider.GetRequiredService<ILogger<TestMerchantService>>();
                return new TestMerchantService(_merchantApiClient, logger);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _minioContainer.StartAsync();
        MinioEndpoint = $"{_minioContainer.Hostname}:{_minioContainer.GetMappedPublicPort(9000)}";
    }

    public new async Task DisposeAsync()
    {
        await _minioContainer.DisposeAsync();
        _merchantApiClient?.Dispose();
        await _merchantApiFactory.DisposeAsync();
        await base.DisposeAsync();
    }
}