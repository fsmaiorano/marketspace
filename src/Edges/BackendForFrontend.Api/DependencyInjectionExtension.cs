using BackendForFrontend.Api.Merchant.Services;
using BackendForFrontend.Api.Basket.Services;
using BackendForFrontend.Api.Catalog.Services;
using BackendForFrontend.Api.Order.Services;
using BuildingBlocks.Http;
using BackendForFrontend.Api.User.Services;

namespace BackendForFrontend.Api;

public static class DependencyInjectionExtension
{
    extension(IServiceCollection services)
    {
        public void AddApplication(IConfiguration configuration)
        {
            AddUseCases(services);
        }

        public void AddInfrastructure(IConfiguration configuration)
        {
            AddServices(services, configuration);
        }
    }

    private static void AddUseCases(IServiceCollection services)
    {
    }

    private static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        string? merchantUrl = configuration["Services:MerchantService:BaseUrl"];
        services.AddHttpClient<MerchantService>(client =>
            {
                client.BaseAddress = new Uri(merchantUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        string? basketUrl = configuration["Services:BasketService:BaseUrl"];
        services.AddHttpClient<BasketService>(client =>
            {
                client.BaseAddress = new Uri(basketUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        string? catalogUrl = configuration["Services:CatalogService:BaseUrl"];
        services.AddHttpClient<CatalogService>(client =>
            {
                client.BaseAddress = new Uri(catalogUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        string? orderUrl = configuration["Services:OrderService:BaseUrl"];
        services.AddHttpClient<OrderService>(client =>
            {
                client.BaseAddress = new Uri(orderUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        string? userUrl = configuration["Services:UserService:BaseUrl"];
        services.AddHttpClient<UserService>(client =>
            {
                client.BaseAddress = new Uri(userUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();
    }
}