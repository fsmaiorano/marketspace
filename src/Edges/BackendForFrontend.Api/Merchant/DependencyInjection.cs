using BackendForFrontend.Api.Merchant.Contracts;
using BackendForFrontend.Api.Merchant.Services;
using BackendForFrontend.Api.Merchant.UseCases;

namespace BackendForFrontend.Api.Merchant;

public static class DependencyInjection
{
    public static IServiceCollection AddMerchantServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IMerchantUseCase, MerchantUseCase>();
        services.AddHttpClient<IMerchantService, MerchantService>();
        return services;
    }
}