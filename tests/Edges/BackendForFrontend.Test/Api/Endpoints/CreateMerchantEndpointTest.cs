namespace BackendForFrontend.Test.Api.Endpoints;

public class CreateMerchantEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Created_Successfully()
    {
        CreateMerchantRequest request = BackendForFrontendBuilder.CreateMerchantRequestFaker();
        
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/merchant", request);
        response.EnsureSuccessStatusCode();
        
        Result<CreateMerchantResponse>? result = await response.Content.ReadFromJsonAsync<Result<CreateMerchantResponse>>();
        result?.Data?.MerchantId.Should().NotBeEmpty();
        result?.IsSuccess.Should().BeTrue();
    }
}