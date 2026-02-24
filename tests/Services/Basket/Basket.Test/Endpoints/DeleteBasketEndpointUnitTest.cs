namespace Basket.Test.Endpoints;

public class DeleteBasketEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Deleted_Successfully()
    {
        ShoppingCartEntity basket = await fixture.CreateBasket();
        
        DeleteBasketCommand command = BasketBuilder.CreateDeleteBasketCommandFaker(basket.Username).Generate();
        HttpResponseMessage response = await DoDelete($"/basket/{command.Username}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
