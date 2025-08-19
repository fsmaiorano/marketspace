using Builder;
using BuildingBlocks;
using Order.Api.Application.Order.GetOrderById;
using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;
using Order.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;

namespace Order.Test.Api.Endpoints;

public class GetOrderByIdEndpointTest(OrderApiFactory factory) : IClassFixture<OrderApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IGetOrderByIdHandler> _mockHandler = new();
    private readonly OrderApiFactory _factory = factory;

    [Fact]
    public async Task Returns_Ok_When_Order_Exists()
    {
        Guid orderId = Guid.NewGuid();
        GetOrderByIdQuery query = new GetOrderByIdQuery(orderId);
        GetOrderByIdResult result = new GetOrderByIdResult
        {
            Id = Guid.NewGuid(),
            Name = "Test Order",
            Description = "This is a test order",
            ImageUrl = "http://example.com/image.jpg",
            Price = 99.99m,
            Categories = new List<string> { "Category1", "Category2" }
        };


        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderByIdQuery>()))
            .ReturnsAsync(Result<GetOrderByIdResult>.Success(result));

        Result<GetOrderByIdResult> response = await _mockHandler.Object.HandleAsync(query);
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_NotFound_When_Order_Does_Not_Exist()
    {
        Guid orderId = Guid.NewGuid();
        GetOrderByIdQuery query = new GetOrderByIdQuery(orderId);
        Result<GetOrderByIdResult> result =
            Result<GetOrderByIdResult>.Failure($"Order with ID {query.Id} not found.");

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderByIdQuery>()))
            .ReturnsAsync(result);

        Result<GetOrderByIdResult> response = await _mockHandler.Object.HandleAsync(query);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be($"Order with ID {query.Id} not found.");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        GetOrderByIdQuery query = new GetOrderByIdQuery(Guid.NewGuid());
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderByIdQuery>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(query);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Get_Order_By_Id_Endpoint()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        OrderDbContext dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        OrderEntity? order = OrderBuilder.CreateOrderFaker().Generate();

        order.Id = OrderId.Of(Guid.NewGuid());

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        GetOrderByIdResult result = new GetOrderByIdResult
        {
            Id = order.Id.Value,
            Name = "Test Order",
            Description = "This is a test order",
            ImageUrl = "http://example.com/image.jpg",
            Price = 99.99m,
            Categories = new List<string> { "Category1", "Category2" }
        };

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderByIdQuery>()))
            .ReturnsAsync(Result<GetOrderByIdResult>.Success(result));

        HttpRequestMessage request = new(HttpMethod.Get, $"/order/{order.Id.Value}");
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Result<GetOrderByIdResult>? responseResult =
            await response.Content.ReadFromJsonAsync<Result<GetOrderByIdResult>>();
        responseResult?.Data.Should().NotBeNull();
    }
}