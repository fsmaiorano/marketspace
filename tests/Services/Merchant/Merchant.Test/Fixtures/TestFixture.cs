using BaseTest.Fixtures;

namespace Merchant.Test.Fixtures;

public sealed class TestFixture : BaseTestFixture<MerchantProgram>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
    }
}