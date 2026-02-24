using Basket.Api.Endpoints.Dto;

namespace Basket.Test.Endpoints;

public class GetBasketByIdEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Basket_Exists()
    {
        ShoppingCartEntity basket = await fixture.CreateBasket();

        HttpResponseMessage response = await DoGet($"/basket/{basket.Username}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ShoppingCartDto? result = await response.Content.ReadFromJsonAsync<ShoppingCartDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.Equal(basket.Username, result.Username);
    }
}