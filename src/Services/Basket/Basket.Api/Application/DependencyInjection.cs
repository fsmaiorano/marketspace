using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Application.Basket.DeleteBasket;
using Basket.Api.Application.Basket.GetBasketById;
using Basket.Api.Domain.Repositories;
using Basket.Api.Infrastructure.Data.Repositories;

namespace Basket.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IBasketRepository, BasketRepository>();
        services.AddScoped<ICreateBasketHandler, CreateBasketHandler>();
        services.AddScoped<IDeleteBasketHandler, DeleteBasketHandler>();
        services.AddScoped<IGetBasketByIdHandler, GetBasketByIdHandler>();
        return services;
    }
}