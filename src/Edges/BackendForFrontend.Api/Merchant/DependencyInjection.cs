using BackendForFrontend.Api.Merchant.Services;
using BackendForFrontend.Api.Merchant.UseCases;

namespace BackendForFrontend.Api.Merchant;

public static class DependencyInjection
{
    public static IServiceCollection AddMerchantServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IMerchantUseCase, MerchantUseCase>();
        
        // Register HttpClient for MerchantService
        services.AddHttpClient<IMerchantService, MerchantService>();
        
        return services;
    }
}