using Basket.Api.Application.Basket.GetBasketById;
using Basket.Test.Base;
using Basket.Test.Fixtures;
using Builder;
using BuildingBlocks;
using System.Net;
using System.Net.Http.Json;

namespace Basket.Test.Endpoints;

public class GetBasketByIdEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Basket_Exists()
    {
        string username = Faker.Internet.UserName();
        HttpResponseMessage response = await DoGet($"/basket/{username}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Result<GetBasketByIdResult>? result = await response.Content.ReadFromJsonAsync<Result<GetBasketByIdResult>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }
}
