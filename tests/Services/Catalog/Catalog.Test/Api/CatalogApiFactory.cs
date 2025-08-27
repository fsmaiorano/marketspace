using Catalog.Api;
using Catalog.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog.Extensions.Hosting;
using BuildingBlocks.Storage.Minio;
using Catalog.Test.Mocks;

namespace Catalog.Test.Api;

public class CatalogApiFactory : WebApplicationFactory<CatalogProgram>, IAsyncLifetime
{
    public CatalogApiFactory()
    {
        // No longer need Minio container since we're using a mock
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            List<ServiceDescriptor> descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            (d.ServiceType == typeof(DbContextOptions<CatalogDbContext>) ||
                             d.ServiceType == typeof(CatalogDbContext) ||
                             d.ServiceType == typeof(ICatalogDbContext) ||
                             // d.ServiceType == typeof(MinioClient) ||
                             // d.ServiceType == typeof(IMinioClient) ||
                             d.ServiceType == typeof(IMinioBucket) ||
                             d.ServiceType.FullName.Contains(nameof(CatalogDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(ICatalogDbContext)) ||
                             d.ServiceType.FullName.Contains("EntityFramework") ||
                             d.ServiceType.FullName.Contains("Npgsql") ||
                             d.ServiceType.FullName.Contains("Minio")))
                .ToList();

            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.AddScoped<ICatalogDbContext, CatalogDbContext>();

            // Use mock Minio bucket instead of real container
            services.Replace(ServiceDescriptor.Scoped<IMinioBucket, MockMinioBucket>());

            services.RemoveAll<ILoggerFactory>();
            services.TryAddSingleton<DiagnosticContext>();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        });
    }

    public async Task InitializeAsync()
    {
        // No longer need to start Minio container
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // No longer need to dispose Minio container
        await base.DisposeAsync();
    }
}