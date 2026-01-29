using Basket.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Order.Api.Infrastructure.Data;
using Serilog.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Minio;
using CatalogTestFixture = Catalog.Test.Fixtures.TestFixture;

namespace BackendForFrontend.Test.Api;

public class BackendForFrontendFactory : WebApplicationFactory<BackendForFrontendProgram>, IAsyncLifetime
{
    private readonly HttpClient _merchantApiClient;
    private readonly HttpClient _orderApiClient;
    private readonly HttpClient _basketApiClient;
    private readonly HttpClient _catalogApiClient;

    public BackendForFrontendFactory()
    {
        Merchant.Test.Fixtures.TestFixture merchantTestFixture = new();
        _merchantApiClient = merchantTestFixture.CreateClient();

        Order.Test.Fixtures.TestFixture orderTestFixture = new();
        _orderApiClient = orderTestFixture.CreateClient();

        Basket.Test.Fixtures.TestFixture basketTestFixture = new();
        _basketApiClient = basketTestFixture.CreateClient();

        CatalogTestFixture catalogTestFixture = new();
        _catalogApiClient = catalogTestFixture.CreateClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
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

            // Remove Serilog services
            services.RemoveAll<ILoggerFactory>();
            services.RemoveAll(typeof(ILogger<>));
            services.RemoveAll<Serilog.ILogger>();
            services.RemoveAll<DiagnosticContext>();

            // Add simple console logging for tests
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Warning);
            });

            services.AddDbContext<BasketDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.AddScoped<IBasketDbContext, BasketDbContext>();

            services.AddDbContext<MerchantDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            // Garantir que o mock seja registrado (Replace não adiciona se já foi removido)
            services.AddScoped<IMinioBucket, MockMinioBucket>();

            services.AddScoped<Basket.Test.Fixtures.TestFixture>();
            services.AddScoped<Order.Test.Fixtures.TestFixture>();
            services.AddScoped<CatalogTestFixture>();

            services.AddScoped<IMerchantService>(provider =>
            {
                IAppLogger<TestMerchantService> logger =
                    provider.GetRequiredService<IAppLogger<TestMerchantService>>();
                return new TestMerchantService(_merchantApiClient, logger);
            });

            services.AddScoped<IBasketService>(provider =>
            {
                IAppLogger<TestBasketService> logger =
                    provider.GetRequiredService<IAppLogger<TestBasketService>>();
                return new TestBasketService(_basketApiClient, logger);
            });

            services.AddScoped<ICatalogService>(provider =>
            {
                IAppLogger<TestCatalogService> logger =
                    provider.GetRequiredService<IAppLogger<TestCatalogService>>();
                return new TestCatalogService(_catalogApiClient, logger);
            });

            services.AddScoped<IOrderService>(provider =>
            {
                IAppLogger<TestOrderService> logger =
                    provider.GetRequiredService<IAppLogger<TestOrderService>>();
                return new TestOrderService(_orderApiClient, logger);
            });

            // Ensure checkout repository uses the in-memory Order API test client instead of calling localhost
            services.AddScoped<Basket.Api.Domain.Repositories.ICheckoutHttpRepository>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<IAppLogger<Basket.Api.Infrastructure.Http.Repositories.CheckoutHttpRepository>>();
                return new Basket.Api.Infrastructure.Http.Repositories.CheckoutHttpRepository(_orderApiClient, config, logger);
            });
        });
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}