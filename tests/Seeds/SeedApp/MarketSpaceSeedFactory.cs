using Catalog.Api.Infrastructure.Data;

namespace SeedApp;

public class MarketSpaceSeedFactory
{
    private const string MerchantDbConnectionString =
        "Server=localhost;Port=5436;Database=MerchantDb;User Id=postgres;Password=postgres;Include Error Detail=true";
    
    private const string CatalogDbConnectionString =
        "Server=localhost;Port=5432;Database=CatalogDb;User Id=postgres;Password=postgres;Include Error Detail=true";

    public IServiceProvider Services { get; private set; }

    public MarketSpaceSeedFactory()
    {
        IHost host = CreateHost();
        Services = host.Services;
    }

    private IHost CreateHost()
    {
        IHostBuilder builder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<MerchantDbContext>(options =>
                    options.UseNpgsql(MerchantDbConnectionString));
                
                services.AddDbContext<CatalogDbContext>(options =>
                    options.UseNpgsql(CatalogDbConnectionString));

                services.AddScoped<IMerchantDbContext, MerchantDbContext>();
                services.AddScoped<ICatalogDbContext, CatalogDbContext>();
            });

        return builder.Build();
    }
}