using Basket.Api.Infrastructure.Data;
using Bogus;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Infrastructure.Data;
using Merchant.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Order.Api.Infrastructure.Data;
using Serilog.Extensions.Hosting;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using User.Api.Data;
using Xunit;

namespace BaseTest.Fixtures;

public class BaseTestFixture<T> : WebApplicationFactory<T>, IAsyncLifetime where T : class
{
    private HttpClient? _httpClient;
    public readonly Faker Faker = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            List<ServiceDescriptor> descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            (d.ServiceType == typeof(DbContextOptions<BasketDbContext>) ||
                             d.ServiceType == typeof(DbContextOptions<MerchantDbContext>) ||
                             d.ServiceType == typeof(DbContextOptions<CatalogDbContext>) ||
                             d.ServiceType == typeof(DbContextOptions<OrderDbContext>) ||
                             d.ServiceType == typeof(DbContextOptions<UserDbContext>) ||
                             d.ServiceType.FullName.Contains(nameof(BasketDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IBasketDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(CatalogDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(ICatalogDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(MerchantDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IMerchantDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(OrderDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IOrderDbContext)) ||
                             d.ServiceType == typeof(BasketDbContext) ||
                             d.ServiceType == typeof(IBasketDbContext) ||
                             d.ServiceType == typeof(CatalogDbContext) ||
                             d.ServiceType == typeof(ICatalogDbContext) ||
                             d.ServiceType == typeof(IMinioBucket) ||
                             d.ServiceType == typeof(OrderDbContext) ||
                             d.ServiceType == typeof(IOrderDbContext) ||
                             d.ServiceType == typeof(MerchantDbContext) ||
                             d.ServiceType == typeof(IMerchantDbContext) ||
                             d.ServiceType.FullName.Contains("Minio") ||
                             d.ServiceType.FullName.Contains("EntityFramework") ||
                             d.ServiceType.FullName.Contains("Npgsql")
                            ))
                .ToList();

            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<BasketDbContext>(options =>
                options.UseInMemoryDatabase("BasketInMemoryDbForTesting"));

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseInMemoryDatabase("CatalogInMemoryDbForTesting"));

            services.AddDbContext<MerchantDbContext>(options =>
                options.UseInMemoryDatabase("MerchantInMemoryDbForTesting"));

            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase("OrderInMemoryDbForTesting"));

            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase("UserInMemoryDbForTesting"));

            services.AddScoped<UserDbContext>();
            services.AddScoped<IOrderDbContext, OrderDbContext>();
            services.AddScoped<IMerchantDbContext, MerchantDbContext>();
            services.AddScoped<ICatalogDbContext, CatalogDbContext>();
            services.AddScoped<IBasketDbContext, BasketDbContext>();

            services.RemoveAll<ILoggerFactory>();
            services.RemoveAll(typeof(ILogger<>));
            services.RemoveAll<Serilog.ILogger>();
            services.RemoveAll<DiagnosticContext>();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Warning);
            });
        });

        builder.UseDefaultServiceProvider((context, options) =>
        {
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });
    }

    private HttpClient GetHttpClient()
    {
        return _httpClient ??= CreateClient();
    }

    public async Task<HttpResponseMessage> DoPost(string method, object request, string token = "",
        string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.PostAsJsonAsync(method, request);
    }

    public async Task<HttpResponseMessage> DoGet(string method, string token = "", string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.GetAsync(method);
    }

    public async Task<HttpResponseMessage> DoPut(string method, object request, string token = "",
        string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.PutAsJsonAsync(method, request);
    }

    public async Task<HttpResponseMessage> DoPatch(string method, object request, string token = "",
        string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.PatchAsJsonAsync(method, request);
    }

    public async Task<HttpResponseMessage> DoDelete(string method, string token = "", string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.DeleteAsync(method);
    }

    private static void AuthorizeRequest(string token, HttpClient client)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = null;
            return;
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static void ChangeRequestCulture(string culture, HttpClient client)
    {
        client.DefaultRequestHeaders.AcceptLanguage.Clear();
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
    }

    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public Task DisposeAsync()
    {
        throw new NotImplementedException();
    }
}