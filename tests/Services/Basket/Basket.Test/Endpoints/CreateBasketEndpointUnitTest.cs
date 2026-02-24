namespace Basket.Test.Endpoints;

public class CreateBasketEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Created_Successfully()
    {
        CreateBasketCommand command = BasketBuilder.CreateBasketCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/basket", command);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}