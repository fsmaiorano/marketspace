using BuildingBlocks.Storage.Minio;
using Catalog.Api.Application.Catalog.CreateCatalog;
using Catalog.Api.Application.Catalog.DeleteCatalog;
using Catalog.Api.Application.Catalog.GetCatalog;
using Catalog.Api.Application.Catalog.GetCatalogById;
using Catalog.Api.Application.Catalog.UpdateCatalog;
using Catalog.Api.Application.Config;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Data.Repositories;
using Minio;

namespace Catalog.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StorageSettings>(
            configuration.GetSection("Storage:Minio"));

        // Only register production MinIO services if a test or other caller hasn't already
        // provided an implementation (tests typically register a mock).
        if (!services.Any(s => s.ServiceType == typeof(IMinioBucket)))
        {
            IMinioClient? minio = new MinioClient()
                .WithEndpoint(configuration.GetSection("Storage:Minio:Endpoint").Value)
                .WithCredentials(configuration.GetSection("Storage:Minio:AccessKey").Value,
                    configuration.GetSection("Storage:Minio:SecretKey").Value)
                .Build();

            services.AddSingleton<IMinioClient>(_ => minio);
            services.AddScoped<IMinioBucket, MinioBucket>();
        }

        services.AddScoped<ICatalogRepository, CatalogRepository>();
        
        services.AddScoped<ICreateCatalogHandler, CreateCatalogHandler>();
        services.AddScoped<IUpdateCatalogHandler, UpdateCatalogHandler>();
        services.AddScoped<IDeleteCatalogHandler, DeleteCatalogHandler>();
        services.AddScoped<IGetCatalogByIdHandler, GetCatalogByIdHandler>();
        services.AddScoped<IGetCatalogHandler, GetCatalogHandler>();
        
        return services;
    }
}