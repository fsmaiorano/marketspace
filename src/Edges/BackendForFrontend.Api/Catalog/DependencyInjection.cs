using BackendForFrontend.Api.Catalog.Services;
using BackendForFrontend.Api.Catalog.UseCases;

namespace BackendForFrontend.Api.Catalog;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogServices(this IServiceCollection services)
    {
        services.AddScoped<CatalogUseCase>();
        services.AddHttpClient<CatalogService>();
        return services;
    }
}
