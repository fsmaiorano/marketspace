namespace Basket.Test.Endpoints;

public class DeleteBasketEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    private readonly TestFixture _fixture = fixture;

    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Deleted_Successfully()
    {
        ShoppingCartEntity fakerEntity = BasketBuilder.CreateShoppingCartFaker().Generate();
        _fixture.BasketDbContext.ShoppingCarts.Add(fakerEntity);
        await _fixture.BasketDbContext.SaveChangesAsync();
        
        DeleteBasketCommand command = BasketBuilder.CreateDeleteBasketCommandFaker(fakerEntity.Username).Generate();
        HttpResponseMessage response = await _fixture.DoDelete($"/basket/{command.Username}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Result<DeleteBasketResult>? result = await response.Content.ReadFromJsonAsync<Result<DeleteBasketResult>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}
