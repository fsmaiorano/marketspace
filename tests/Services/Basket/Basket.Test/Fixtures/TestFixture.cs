using BaseTest.Fixtures;
using Basket.Api;
using Microsoft.AspNetCore.TestHost;

namespace Basket.Test.Fixtures;

public sealed class TestFixture : BaseTestFixture<BasketProgram>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
        });
    }
}