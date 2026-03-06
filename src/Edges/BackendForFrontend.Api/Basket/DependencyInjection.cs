using BackendForFrontend.Api.Basket.Services;
using BackendForFrontend.Api.Basket.UseCases;

namespace BackendForFrontend.Api.Basket;

public static class DependencyInjection
{
    public static IServiceCollection AddBasketServices(this IServiceCollection services)
    {
        services.AddScoped<BasketUseCase>();
        services.AddHttpClient<BasketService>();
        return services;
    }
}
