using BaseTest.Fixtures;

namespace Order.Test.Fixtures;

public sealed class TestFixture : BaseTestFixture<OrderProgram>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
    }
}
