using Merchant.Api.Application.Merchant.CreateMerchant;
using Merchant.Api.Application.Merchant.DeleteMerchant;
using Merchant.Api.Application.Merchant.GetMerchantById;
using Merchant.Api.Application.Merchant.UpdateMerchant;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Infrastructure.Data.Repositories;

namespace Merchant.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IMerchantRepository, MerchantRepository>();
        
        services.AddScoped<CreateMerchant>();
        services.AddScoped<UpdateMerchant>();
        services.AddScoped<DeleteMerchant>();
        services.AddScoped<GetMerchantById>();
        
        return services;
    }
}