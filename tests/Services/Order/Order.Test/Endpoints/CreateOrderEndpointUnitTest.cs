using Order.Test.Fixtures;
using System.Net;

namespace Order.Test.Endpoints;

public class CreateOrderEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Order_Is_Created_Successfully()
    {
        CreateOrderCommand command = OrderBuilder.CreateCreateOrderCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/order", command);
        Assert.True(response.StatusCode.Equals(HttpStatusCode.Created));
    }
}