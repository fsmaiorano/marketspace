namespace Basket.Test.Endpoints;

public class GetBasketByIdEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    private readonly TestFixture _fixture = fixture;

    [Fact]
    public async Task Returns_Ok_When_Basket_Exists()
    {
        ShoppingCartEntity fakerEntity = BasketBuilder.CreateShoppingCartFaker().Generate();
        _fixture.BasketDbContext.ShoppingCarts.Add(fakerEntity);
        await _fixture.BasketDbContext.SaveChangesAsync();

        HttpResponseMessage response = await _fixture.DoGet($"/basket/{fakerEntity.Username}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Result<GetBasketByIdResult>? result = await response.Content.ReadFromJsonAsync<Result<GetBasketByIdResult>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }
}