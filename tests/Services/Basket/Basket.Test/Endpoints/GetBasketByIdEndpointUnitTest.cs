namespace Basket.Test.Endpoints;

public class GetBasketByIdEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Basket_Exists()
    {
        ShoppingCartEntity basket = await fixture.CreateBasket();

        HttpResponseMessage response = await DoGet($"/basket/{basket.Username}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Result<GetBasketByIdResult>? result = await response.Content.ReadFromJsonAsync<Result<GetBasketByIdResult>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }
}