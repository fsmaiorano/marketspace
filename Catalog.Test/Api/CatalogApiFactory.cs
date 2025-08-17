using Catalog.Api;
using Catalog.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Test.Api;

public class CatalogApiFactory : WebApplicationFactory<CatalogProgram>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            List<ServiceDescriptor> descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            (d.ServiceType == typeof(DbContextOptions<CatalogDbContext>) ||
                             d.ServiceType == typeof(CatalogDbContext) ||
                             d.ServiceType == typeof(ICatalogDbContext) ||
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
        });
    }
}