using Merchant.Api.Application.Merchant.GetMerchantById;
using Merchant.Test.Fixtures;

namespace Merchant.Test.Endpoints;

public class GetMerchantByIdEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Merchant_Exists()
    {
        MerchantEntity? merchant = await fixture.CreateMerchant();

        GetMerchantByIdQuery query = new(merchant.Id.Value);
        HttpResponseMessage response = await DoGet($"/merchant/{query.Id}");
        Result<GetMerchantByIdResult>? result =
            await response.Content.ReadFromJsonAsync<Result<GetMerchantByIdResult>>();

        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}