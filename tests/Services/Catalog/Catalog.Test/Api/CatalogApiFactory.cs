using Catalog.Api;
using Catalog.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Testcontainers.Minio;

namespace Catalog.Test.Api;

public class CatalogApiFactory : WebApplicationFactory<CatalogProgram>, IAsyncLifetime
{
    private readonly MinioContainer _minioContainer;
    public string MinioEndpoint => _minioContainer.GetConnectionString();
    public string AccessKey => "admin";
    public string SecretKey => "admin123";

    public CatalogApiFactory()
    {
        _minioContainer = new MinioBuilder()
            .WithUsername(AccessKey)
            .WithPassword(SecretKey)
            .Build();
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
                             d.ServiceType == typeof(MinioClient) ||
                             d.ServiceType.FullName.Contains(nameof(CatalogDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(ICatalogDbContext)) ||
                             d.ServiceType.FullName.Contains("EntityFramework") ||
                             d.ServiceType.FullName.Contains("Npgsql")))
                .ToList();

            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<CatalogDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.AddScoped<ICatalogDbContext, CatalogDbContext>();

            services.AddSingleton(_ => new MinioClient()
                .WithEndpoint(_minioContainer.GetConnectionString())
                .WithCredentials(AccessKey, SecretKey)
                .Build());
        });
    }

    public async Task InitializeAsync() => await _minioContainer.StartAsync();
    public async Task DisposeAsync() => await _minioContainer.DisposeAsync();
}