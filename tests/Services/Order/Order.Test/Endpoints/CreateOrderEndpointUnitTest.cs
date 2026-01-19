using Order.Test.Base;
using Order.Test.Fixtures;

namespace Order.Test.Endpoints;

public class CreateOrderEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    private readonly Mock<ICreateOrderHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Order_Is_Created_Successfully()
    {
        CreateOrderCommand command = OrderBuilder.CreateCreateOrderCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/order", command);
        Result<CreateOrderResult>? result = await response.Content.ReadFromJsonAsync<Result<CreateOrderResult>>();
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}