namespace Merchant.Test.Api;

public class MerchantApiFactory : WebApplicationFactory<MerchantProgram>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            List<ServiceDescriptor> descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            (d.ServiceType == typeof(DbContextOptions<MerchantDbContext>) ||
                             d.ServiceType == typeof(MerchantDbContext) ||
                             d.ServiceType == typeof(IMerchantDbContext) ||
                             d.ServiceType.FullName.Contains(nameof(MerchantDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IMerchantDbContext)) ||
                             d.ServiceType.FullName.Contains("EntityFramework") ||
                             d.ServiceType.FullName.Contains("Npgsql")))
                .ToList();

            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<MerchantDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.AddScoped<IMerchantDbContext, MerchantDbContext>();
        });
    }
}