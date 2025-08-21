using Basket.Api;
using Basket.Api.Domain.Repositories;
using Basket.Api.Infrastructure.Data;
using Basket.Api.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Mongo2Go;
using MongoDB.Driver;
using Serilog;
using Serilog.Extensions.Hosting;

namespace Basket.Test.Api;

public class BasketApiFactory : WebApplicationFactory<BasketProgram>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            List<ServiceDescriptor> descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            (d.ServiceType == typeof(BasketDbContext) ||
                             d.ServiceType == typeof(IBasketDbContext) ||
                             d.ServiceType.FullName.Contains(nameof(BasketDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IBasketDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IMongoClient)) ||
                             d.ServiceType.FullName.Contains("EntityFramework") ||
                             d.ServiceType.FullName.Contains("Npgsql")))
                .ToList();

            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            MongoDbRunner? runner = MongoDbRunner.Start();
            
            services.AddSingleton<IMongoClient>(sp => new MongoClient(runner.ConnectionString));
            
            services.AddScoped(sp =>
            {
                IMongoClient client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase("BasketInMemoryDbForTesting");
            });

            services.AddScoped<IBasketRepository, BasketRepository>();

            services.RemoveAll<ILoggerFactory>();
            services.TryAddSingleton<DiagnosticContext>();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        });
    }
}

public interface IBasketDbContext
{
}