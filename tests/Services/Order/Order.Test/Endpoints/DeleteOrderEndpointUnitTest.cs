using Order.Test.Fixtures;
using System.Net;

namespace Order.Test.Endpoints;

public class DeleteOrderEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Delete_Order_Endpoint()
    {
        OrderEntity? order = await fixture.CreateOrder();
        HttpResponseMessage response = await DoDelete($"/order/{order.Id.Value}");
        Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));
    }
}
