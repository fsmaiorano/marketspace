using Order.Api.Application.Order.PatchOrderStatus;
using Order.Test.Fixtures;

namespace Order.Test.Endpoints;

public class PatchOrderStatusEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Patch_Order_Status_Endpoint()
    {
        OrderEntity? order = OrderBuilder.CreateOrderFaker().Generate();

        Context.Orders.Add(order);
        await Context.SaveChangesAsync();

        PatchOrderStatusCommand command = OrderBuilder.CreatePatchOrderStatusCommandFaker(order.Id.Value).Generate();

        HttpResponseMessage response = await DoPatch("/order/status", command);
        Result<PatchOrderStatusResult>? result = await response.Content.ReadFromJsonAsync<Result<PatchOrderStatusResult>>();

        OrderEntity? updatedOrder = await Context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == order.Id);
            
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
        updatedOrder?.Status.Should().Be(command.Status);
    }
}