using Order.Test.Fixtures;

namespace Order.Test.Endpoints;

public class DeleteOrderEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Delete_Order_Endpoint()
    {
        OrderEntity? order = await fixture.CreateOrder();

        HttpResponseMessage response = await DoDelete($"/order/{order.Id.Value}");
        response.EnsureSuccessStatusCode();
        Result<DeleteOrderResult>? result = await response.Content.ReadFromJsonAsync<Result<DeleteOrderResult>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
