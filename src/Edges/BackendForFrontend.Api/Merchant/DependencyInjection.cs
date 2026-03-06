using BackendForFrontend.Api.Merchant.Services;
using BackendForFrontend.Api.Merchant.UseCases;

namespace BackendForFrontend.Api.Merchant;

public static class DependencyInjection
{
    public static IServiceCollection AddMerchantServices(this IServiceCollection services)
    {
        services.AddScoped<MerchantUseCase>();
        services.AddHttpClient<MerchantService>();
        return services;
    }
}