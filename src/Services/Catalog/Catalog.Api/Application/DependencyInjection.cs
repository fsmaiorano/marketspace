using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Storage.Minio;
using Catalog.Api.Application.Catalog.CreateCatalog;
using Catalog.Api.Application.Catalog.DeleteCatalog;
using Catalog.Api.Application.Catalog.GetCatalog;
using Catalog.Api.Application.Catalog.GetCatalogById;
using Catalog.Api.Application.Catalog.GetCatalogByMerchantId;
using Catalog.Api.Application.Catalog.UpdateCatalog;
using Catalog.Api.Application.Catalog.UpdateStock;
using Catalog.Api.Application.HostedService;
using Catalog.Api.Application.Subscribers;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Data.Repositories;
using Minio;

namespace Catalog.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services.All(s => s.ServiceType != typeof(IMinioBucket)))
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

        services.AddScoped<CreateCatalog>();
        services.AddScoped<UpdateCatalog>();
        services.AddScoped<DeleteCatalog>();
        services.AddScoped<GetCatalogById>();
        services.AddScoped<GetCatalog>();
        services.AddScoped<GetCatalogByMerchantId>();
        services.AddScoped<UpdateStock>();

        services.AddEventBus(configuration);
        services.AddScoped<OnOrderCreatedSubscriber>();
        services.AddScoped<OnPaymentStatusChangedSubscriber>();
        services.AddHostedService<IntegrationEventSubscriptionService>();

        return services;
    }
}