using Merchant.Api.Application.Merchant.CreateMerchant;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Infrastructure.Data.Repositories;

namespace Merchant.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IMerchantRepository, MerchantRepository>();

        services.AddScoped<ICreateMerchantHandler, CreateMerchantHandler>();

        return services;
    }
}