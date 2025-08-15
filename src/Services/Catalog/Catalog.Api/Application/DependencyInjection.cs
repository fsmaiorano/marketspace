using Catalog.Api.Application.Catalog.CreateCatalog;
using Catalog.Api.Application.Catalog.DeleteCatalog;
using Catalog.Api.Application.Catalog.GetCatalogById;
using Catalog.Api.Application.Catalog.UpdateCatalog;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Data.Repositories;

namespace Catalog.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ICreateCatalogHandler, CreateCatalogHandler>();
        services.AddScoped<IUpdateCatalogHandler, UpdateCatalogHandler>();
        services.AddScoped<IDeleteCatalogHandler, DeleteCatalogHandler>();
        services.AddScoped<IGetCatalogByIdHandler, GetCatalogByIdHandler>();
        return services;
    }
}