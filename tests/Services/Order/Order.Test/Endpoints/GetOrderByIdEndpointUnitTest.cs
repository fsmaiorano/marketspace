using Order.Api.Endpoints.Dto;
using Order.Test.Fixtures;

namespace Order.Test.Endpoints;

public class GetOrderByIdEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Order_Exists()
    {
        OrderEntity createdOrder = await fixture.CreateOrder();

        GetOrderByIdQuery query = new(createdOrder.Id.Value);
        HttpResponseMessage response = await DoGet($"/order/{query.Id}");
        OrderDto? result =
            await response.Content.ReadFromJsonAsync<OrderDto>();

        Assert.NotNull(result);
    }
}