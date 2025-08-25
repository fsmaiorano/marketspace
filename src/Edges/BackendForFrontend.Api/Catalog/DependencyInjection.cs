using BackendForFrontend.Api.Catalog.Contracts;
using BackendForFrontend.Api.Catalog.Services;
using BackendForFrontend.Api.Catalog.UseCases;

namespace BackendForFrontend.Api.Catalog;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ICatalogUseCase, CatalogUseCase>();
        
        // Register HttpClient for CatalogService
        services.AddHttpClient<ICatalogService, CatalogService>();
        
        return services;
    }
}
