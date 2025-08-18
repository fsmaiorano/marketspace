using Catalog.Api;
using Catalog.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Catalog.Test.Api;

public class CatalogApiFactory : WebApplicationFactory<CatalogProgram>, IAsyncLifetime
{
    private readonly IContainer _minioContainer;
    public string MinioEndpoint { get; private set; } = string.Empty;
    public string AccessKey => "admin";
    public string SecretKey => "admin123";

    public CatalogApiFactory()
    {
        _minioContainer = new ContainerBuilder()
            .WithImage("minio/minio:latest")
            .WithEnvironment("MINIO_ROOT_USER", AccessKey)
            .WithEnvironment("MINIO_ROOT_PASSWORD", SecretKey)
            .WithPortBinding(9000, true)
            .WithCommand("server", "/data", "--console-address", ":9001")
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
                             d.ServiceType == typeof(IMinioClient) ||
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

            services.AddSingleton<IMinioClient>(_ =>
            {
                if (string.IsNullOrEmpty(MinioEndpoint))
                    throw new InvalidOperationException(
                        "MinIO container not initialized. Make sure to call InitializeAsync() first.");

                return new MinioClient()
                    .WithEndpoint(MinioEndpoint)
                    .WithCredentials(AccessKey, SecretKey)
                    .Build();
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _minioContainer.StartAsync();
        ushort port = _minioContainer.GetMappedPublicPort(9000);
        MinioEndpoint = $"localhost:{port}";
    }

    public new async Task DisposeAsync() => await _minioContainer.DisposeAsync();
}