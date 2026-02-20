using Merchant.Test.Fixtures;

namespace Merchant.Test.Endpoints;

public class CreateMerchantEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Created_Successfully()
    {
        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/merchant", command);
        Result<CreateMerchantResult>? result = await response.Content.ReadFromJsonAsync<Result<CreateMerchantResult>>();
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}