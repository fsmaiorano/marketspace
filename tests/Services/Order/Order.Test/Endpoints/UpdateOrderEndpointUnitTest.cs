using Order.Api.Domain.ValueObjects;
using Order.Test.Fixtures;

namespace Order.Test.Endpoints;

public class UpdateOrderEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Update_Order_Endpoint()
    {
        OrderEntity? order = OrderBuilder.CreateOrderFaker().Generate();

        order.Id = OrderId.Of(Guid.CreateVersion7());

        Context.Orders.Add(order);
        await Context.SaveChangesAsync();

        UpdateOrderCommand command = OrderBuilder.CreateUpdateOrderCommandFaker(order.Id.Value).Generate();
        HttpResponseMessage response = await DoPut("/order", command);
        Result<UpdateOrderResult>? result = await response.Content.ReadFromJsonAsync<Result<UpdateOrderResult>>();
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}