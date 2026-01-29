using BaseTest.Fixtures;
using Microsoft.AspNetCore.Hosting;
using User.Api.Api;

namespace User.Test.Fixtures;

public sealed class TestFixture : BaseTestFixture<UserProgram>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
    }
}