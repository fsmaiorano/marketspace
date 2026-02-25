using Merchant.Api.Application.Merchant.DeleteMerchant;
using Merchant.Test.Fixtures;
using System.Net;

namespace Merchant.Test.Endpoints;

public class DeleteMerchantEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Deleted_Successfully()
    {
        MerchantEntity? merchant = await fixture.CreateMerchant();
        
        DeleteMerchantCommand command = MerchantBuilder.CreateDeleteMerchantCommandFaker(merchant.Id.Value);
        HttpResponseMessage response = await DoDelete($"/merchant/{command.Id}");
        Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));
    }
}
