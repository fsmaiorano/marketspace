using Order.Api.Application.Order.PatchOrderStatus;
using Order.Test.Fixtures;
using System.Net;

namespace Order.Test.Endpoints;

public class PatchOrderStatusEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Patch_Order_Status_Endpoint()
    {
        OrderEntity? order = await fixture.CreateOrder();

        PatchOrderStatusCommand command = OrderBuilder.CreatePatchOrderStatusCommandFaker(order.Id.Value).Generate();

        HttpResponseMessage response = await DoPatch("/order/status", command);
        Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));

        OrderEntity? updatedOrder = await Context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        updatedOrder?.Status.Should().Be(command.Status);
    }
}