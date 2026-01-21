using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Application.Dto;
using Basket.Test.Base;
using Basket.Test.Fixtures;
using Builder;
using BuildingBlocks;
using System.Net;
using System.Net.Http.Json;

namespace Basket.Test.Endpoints;

public class CreateBasketEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Created_Successfully()
    {
        CreateBasketCommand command = BasketBuilder.CreateBasketCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/basket", command);


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Result<CreateBasketResult>? result = await response.Content.ReadFromJsonAsync<Result<CreateBasketResult>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.ShoppingCart);
    }
}
