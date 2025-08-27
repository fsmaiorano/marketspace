using BackendForFrontend.Api;
using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Basket.Contracts;
using BackendForFrontend.Api.Basket.Services;
using BackendForFrontend.Api.Catalog.Contracts;
using BackendForFrontend.Api.Catalog.Services;
using BackendForFrontend.Api.Order.Contracts;
using Basket.Api.Infrastructure.Data;
using Basket.Test.Api;
using Catalog.Api.Infrastructure.Data;
using Catalog.Test.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using BackendForFrontend.Api.Order.Services;
using Merchant.Test.Api;
using BackendForFrontend.Test.Mocks;
using BuildingBlocks.Storage.Minio;
using Minio;

namespace BackendForFrontend.Test.Api;

public class BackendForFrontendFactory : WebApplicationFactory<BackendForFrontendProgram>, IAsyncLifetime
{
    private HttpClient _merchantApiClient;
    private HttpClient _orderApiClient;
    private HttpClient _basketApiClient;
    private HttpClient _catalogApiClient;

    public BackendForFrontendFactory()
    {
        MerchantApiFactory merchantApiFactory = new();
        _merchantApiClient = merchantApiFactory.CreateClient();

        OrderApiFactory orderApiFactory = new();
        _orderApiClient = orderApiFactory.CreateClient();

        BasketApiFactory basketApiFactory = new();
        _basketApiClient = basketApiFactory.CreateClient();

        CatalogApiFactory catalogApiFactory = new();
        _catalogApiClient = catalogApiFactory.CreateClient();
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
                             d.ServiceType == typeof(IMinioBucket) ||
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
                             d.ServiceType.FullName.Contains("Npgsql") ||
                             d.ServiceType.FullName.Contains("Minio")) ||
                            d.ServiceKey?.ToString() == nameof(MerchantService) ||
                            d.ServiceKey?.ToString() == nameof(IMerchantService) ||
                            d.ServiceKey?.ToString() == nameof(IOrderService) ||
                            d.ServiceKey?.ToString() == nameof(OrderService) ||
                            d.ServiceKey?.ToString() == nameof(ICatalogService) ||
                            d.ServiceKey?.ToString() == nameof(CatalogService) ||
                            d.ServiceKey?.ToString() == nameof(IBasketService) ||
                            d.ServiceKey?.ToString() == nameof(BasketService)
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

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.Replace(ServiceDescriptor.Scoped<IMinioBucket, MockMinioBucket>());

            services.AddScoped<BasketApiFactory>();
            services.AddScoped<OrderApiFactory>();
            services.AddScoped<CatalogApiFactory>();

            services.AddScoped<IMerchantService>(provider =>
            {
                ILogger<TestMerchantService> logger = provider.GetRequiredService<ILogger<TestMerchantService>>();
                return new TestMerchantService(_merchantApiClient, logger);
            });

            services.AddScoped<IBasketService>(provider =>
            {
                ILogger<TestBasketService> logger = provider.GetRequiredService<ILogger<TestBasketService>>();
                return new TestBasketService(_basketApiClient!, logger);
            });

            services.AddScoped<ICatalogService>(provider =>
            {
                ILogger<TestCatalogService> logger = provider.GetRequiredService<ILogger<TestCatalogService>>();
                return new TestCatalogService(_catalogApiClient, logger);
            });

            services.AddScoped<IOrderService>(provider =>
            {
                ILogger<TestOrderService> logger = provider.GetRequiredService<ILogger<TestOrderService>>();
                return new TestOrderService(_orderApiClient, logger);
            });
        });
    }

    public async Task InitializeAsync()
    {
        // Minio container is no longer used, so nothing to initialize
    }

    public new async Task DisposeAsync()
    {
        // Minio container is no longer used, so nothing to dispose
        await base.DisposeAsync();
    }
}