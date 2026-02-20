using Merchant.Api.Application.Merchant.DeleteMerchant;
using Merchant.Test.Fixtures;

namespace Merchant.Test.Endpoints;

public class DeleteMerchantEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Deleted_Successfully()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker().Generate();
        Context.Merchants.Add(merchant);
        await Context.SaveChangesAsync();
        
        DeleteMerchantCommand command = MerchantBuilder.CreateDeleteMerchantCommandFaker(merchant.Id.Value);
        HttpResponseMessage response = await DoDelete($"/merchant/{command.Id}");
        Result<DeleteMerchantResult>? result = await response.Content.ReadFromJsonAsync<Result<DeleteMerchantResult>>();
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}
