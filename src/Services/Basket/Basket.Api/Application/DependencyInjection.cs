using Basket.Api.Application.Basket.CheckoutBasket;
using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Application.Basket.DeleteBasket;
using Basket.Api.Application.Basket.GetBasketById;
using Basket.Api.Domain.Repositories;
using Basket.Api.Infrastructure.Data.Repositories;
using Basket.Api.Infrastructure.Http.Repositories;

namespace Basket.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IBasketDataRepository, BasketDataRepository>();
        services.AddScoped<ICreateBasketHandler, CreateBasketHandler>();
        services.AddScoped<IDeleteBasketHandler, DeleteBasketHandler>();
        services.AddScoped<IGetBasketByIdHandler, GetBasketByIdHandler>();
        services.AddScoped<ICheckoutBasketHandler, CheckoutBasketHandler>();

        services.AddHttpClient<CheckoutHttpRepository>();

        return services;
    }
}