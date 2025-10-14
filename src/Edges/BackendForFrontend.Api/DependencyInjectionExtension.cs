using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Services;
using BackendForFrontend.Api.Basket.Contracts;
using BackendForFrontend.Api.Basket.Services;
using BackendForFrontend.Api.Catalog.Contracts;
using BackendForFrontend.Api.Catalog.Services;
using BackendForFrontend.Api.Order.Contracts;
using BackendForFrontend.Api.Order.Services;
using BuildingBlocks.Http;

namespace BackendForFrontend.Api;

public static class DependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddUseCases(services);
    }

    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddServices(services, configuration);
    }

    private static void AddUseCases(IServiceCollection services)
    {
    }

    private static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CorrelationIdHandler>();
        services.AddTransient<LoggingHandler>();

        string? merchantUrl = configuration["Services:MerchantService:BaseUrl"];
        services.AddHttpClient<IMerchantService, MerchantService>(client =>
            {
                client.BaseAddress = new Uri(merchantUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        string? basketUrl = configuration["Services:BasketService:BaseUrl"];
        services.AddHttpClient<IBasketService, BasketService>(client =>
            {
                client.BaseAddress = new Uri(basketUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        string? catalogUrl = configuration["Services:CatalogService:BaseUrl"];
        services.AddHttpClient<ICatalogService, CatalogService>(client =>
            {
                client.BaseAddress = new Uri(catalogUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        string? orderUrl = configuration["Services:OrderService:BaseUrl"];
        services.AddHttpClient<IOrderService, OrderService>(client =>
            {
                client.BaseAddress = new Uri(orderUrl ?? throw new InvalidOperationException());
            })
            .AddHttpMessageHandler<CorrelationIdHandler>()
            .AddHttpMessageHandler<LoggingHandler>();
    }
}