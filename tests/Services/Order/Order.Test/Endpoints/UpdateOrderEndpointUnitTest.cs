using Order.Test.Fixtures;
using System.Net;

namespace Order.Test.Endpoints;

public class UpdateOrderEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Update_Order_Endpoint()
    {
        OrderEntity? order = await fixture.CreateOrder();

        UpdateOrderCommand command = OrderBuilder.CreateUpdateOrderCommandFaker(order.Id.Value).Generate();
        HttpResponseMessage response = await DoPut("/order", command);
        Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));
    }
}