using Builder;
using BuildingBlocks;
using Order.Api.Application.Order.UpdateOrder;
using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;
using Order.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Order.Api.Application.Dto;
using System.Net.Http.Json;

namespace Order.Test.Api.Endpoints;

public class UpdateOrderEndpointTest(OrderApiFactory factory) : IClassFixture<OrderApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IUpdateOrderHandler> _mockHandler = new();
    private readonly OrderApiFactory _factory = factory;

    [Fact]
    public async Task Returns_Failure_When_Order_Not_Found()
    {
        Guid orderId = Guid.CreateVersion7();

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateOrderCommand>()))
            .ReturnsAsync(Result<UpdateOrderResult>.Failure("Order not found."));

        UpdateOrderCommand command = new UpdateOrderCommand { Id = orderId };

        Result<UpdateOrderResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Order not found.");
    }

    [Fact]
    public async Task Returns_Failure_When_Exception_Occurs()
    {
        Guid orderId = Guid.CreateVersion7();

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateOrderCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        UpdateOrderCommand command = new UpdateOrderCommand { Id = orderId };

        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);

        await act.Should().ThrowAsync<Exception>().WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Returns_Success_When_Order_Updated()
    {
        Guid orderId = Guid.CreateVersion7();

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<UpdateOrderCommand>()))
            .ReturnsAsync(Result<UpdateOrderResult>.Success(new UpdateOrderResult(isSuccess: true)));

        UpdateOrderCommand command = new UpdateOrderCommand { Id = orderId };

        Result<UpdateOrderResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Data?.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Can_Update_Order_Endpoint()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        OrderDbContext dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        OrderEntity? order = OrderBuilder.CreateOrderFaker().Generate();

        order.Id = OrderId.Of(Guid.CreateVersion7());

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        UpdateOrderCommand command = new UpdateOrderCommand
        {
            Id = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            ShippingAddress = OrderBuilder.CreateAddressDtoFaker().Generate(),
            BillingAddress = OrderBuilder.CreateAddressDtoFaker().Generate(),
            Payment = OrderBuilder.CreatePaymentDtoFaker(),
            Status = order.Status,
            TotalAmount = order.TotalAmount.Value,
            Items = OrderBuilder.CreateOrderItemDtoFaker(10)
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"/order", command);

        UpdateOrderResult? result = await response.Content.ReadFromJsonAsync<UpdateOrderResult>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
    }
}