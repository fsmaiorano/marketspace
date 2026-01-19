using Basket.Api.Application.Basket.DeleteBasket;
using Basket.Test.Base;
using Basket.Test.Fixtures;
using Builder;
using BuildingBlocks;
using System.Net;
using System.Net.Http.Json;

namespace Basket.Test.Endpoints;

public class DeleteBasketEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Deleted_Successfully()
    {
        DeleteBasketCommand command = BasketBuilder.CreateDeleteBasketCommandFaker().Generate();
        HttpResponseMessage response = await DoDelete($"/basket/{command.Username}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Result<DeleteBasketResult>? result = await response.Content.ReadFromJsonAsync<Result<DeleteBasketResult>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}
