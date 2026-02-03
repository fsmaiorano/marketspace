using BaseTest.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payment.Api.Infrastructure.Data;

namespace Payment.Test.Fixtures;

public sealed class TestFixture : BaseTestFixture<Payment.Api.PaymentProgram>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PaymentDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<PaymentDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
            
             // Ensure IPaymentDbContext resolves to the new PaymentDbContext
            var interfaceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPaymentDbContext));
            if(interfaceDescriptor != null) services.Remove(interfaceDescriptor);

            services.AddScoped<IPaymentDbContext>(provider => provider.GetRequiredService<PaymentDbContext>());
        });
    }
}
