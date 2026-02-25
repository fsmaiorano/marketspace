using Merchant.Test.Fixtures;
using System.Net;

namespace Merchant.Test.Endpoints;

public class CreateMerchantEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Created_Successfully()
    {
        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/merchant", command);
        Assert.True(response.StatusCode.Equals(HttpStatusCode.Created));
    }
}