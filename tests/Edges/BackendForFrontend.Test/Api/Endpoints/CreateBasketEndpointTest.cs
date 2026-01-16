namespace BackendForFrontend.Test.Api.Endpoints;

public class CreateBasketEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Created_Successfully()
    {
        CreateBasketRequest request = BackendForFrontendBuilder.CreateBasketRequestFaker();
        
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/basket", request);
        response.EnsureSuccessStatusCode();
        
        Result<CreateBasketResponse>? result = await response.Content.ReadFromJsonAsync<Result<CreateBasketResponse>>();
        result?.Data?.ShoppingCart.Username.Should().NotBeNullOrEmpty();
        result?.IsSuccess.Should().BeTrue();
    }
}
