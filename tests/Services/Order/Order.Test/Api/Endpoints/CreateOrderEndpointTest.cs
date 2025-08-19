using Builder;
using BuildingBlocks;
using FluentAssertions;
using Moq;
using Order.Api.Application.Order.CreateOrder;
using System.Net.Http.Json;

namespace Order.Test.Api.Endpoints;

public class CreateOrderEndpointTest(OrderApiFactory factory) : IClassFixture<OrderApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<ICreateOrderHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Order_Is_Created_Successfully()
    {
        Guid orderId = Guid.NewGuid();

        CreateOrderCommand command = OrderBuilder.CreateCreateOrderCommandFaker().Generate();
        Result<CreateOrderResult>
            result = Result<CreateOrderResult>.Success(new CreateOrderResult(orderId));

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateOrderCommand>()))
            .ReturnsAsync(result);

        Result<CreateOrderResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_Error_Result_When_Handler_Fails()
    {
        CreateOrderCommand command = OrderBuilder.CreateCreateOrderCommandFaker().Generate();
        Result<CreateOrderResult> result = Result<CreateOrderResult>.Failure("Validation error");
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateOrderCommand>()))
            .ReturnsAsync(result);
        Result<CreateOrderResult> response = await _mockHandler.Object.HandleAsync(command);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Validation error");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        CreateOrderCommand command = OrderBuilder.CreateCreateOrderCommandFaker().Generate();
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateOrderCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Create_Order_Endpoint()
    {
        CreateOrderCommand command = OrderBuilder.CreateCreateOrderCommandFaker(customerId: Guid.NewGuid()).Generate();
        
        HttpResponseMessage response = await _client.PostAsJsonAsync("/order", command);
        response.EnsureSuccessStatusCode();

        Result<CreateOrderResult>? result = await response.Content.ReadFromJsonAsync<Result<CreateOrderResult>>();
        result.Should().NotBeNull();
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}