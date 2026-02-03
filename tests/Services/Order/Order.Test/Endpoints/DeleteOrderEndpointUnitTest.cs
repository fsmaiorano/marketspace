using Builder;
using BuildingBlocks;
using Order.Api.Application.Order.DeleteOrder;
using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;
using Order.Test.Base;
using Order.Test.Fixtures;
using FluentAssertions;
using Moq;
using System.Net.Http.Json;

namespace Order.Test.Endpoints;

public class DeleteOrderEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    private readonly Mock<IDeleteOrderHandler> _mockHandler = new();

    [Fact]
    public async Task Can_Delete_Order_Endpoint()
    {
        OrderEntity? order = OrderBuilder.CreateOrderFaker().Generate();
        
        order.Id = OrderId.Of(Guid.CreateVersion7());

        Context.Orders.Add(order);
        await Context.SaveChangesAsync();

        DeleteOrderCommand command = OrderBuilder.CreateDeleteOrderCommandFaker(order.Id.Value).Generate();
        HttpRequestMessage request = new(HttpMethod.Delete, "/order") { Content = JsonContent.Create(command) };
        HttpResponseMessage response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        DeleteOrderResult? result = await response.Content.ReadFromJsonAsync<DeleteOrderResult>();
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Returns_Ok_When_Order_Is_Deleted_Successfully()
    {
        DeleteOrderCommand command = OrderBuilder.CreateDeleteOrderCommandFaker().Generate();
        Result<DeleteOrderResult> result = Result<DeleteOrderResult>.Success(new DeleteOrderResult(true));

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteOrderCommand>()))
            .ReturnsAsync(result);

        Result<DeleteOrderResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_Error_Result_When_Handler_Fails()
    {
        DeleteOrderCommand command = OrderBuilder.CreateDeleteOrderCommandFaker().Generate();
        Result<DeleteOrderResult> result = Result<DeleteOrderResult>.Failure("Validation error");
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteOrderCommand>()))
            .ReturnsAsync(result);
        Result<DeleteOrderResult> response = await _mockHandler.Object.HandleAsync(command);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Validation error");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        DeleteOrderCommand command = OrderBuilder.CreateDeleteOrderCommandFaker().Generate();
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteOrderCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }
}
