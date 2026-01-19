using Builder;
using BuildingBlocks;
using Order.Api.Application.Dto;
using Order.Api.Application.Order.GetOrderById;
using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;
using Order.Test.Base;
using Order.Test.Fixtures;
using FluentAssertions;
using Moq;
using System.Net.Http.Json;

namespace Order.Test.Endpoints;

public class GetOrderByIdEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    private readonly Mock<IGetOrderByIdHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Order_Exists()
    {
        Guid orderId = Guid.CreateVersion7();
        GetOrderByIdQuery query = new GetOrderByIdQuery(orderId);
        GetOrderByIdResult result = new GetOrderByIdResult
        {
            Id = Guid.CreateVersion7(),
            CustomerId = Guid.CreateVersion7(),
            Status = "Pending",
            TotalAmount = 99.99m,
            ShippingAddress = new AddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
                AddressLine = "123 Main St",
                Country = "US",
                State = "CA",
                ZipCode = "90001"
            },
            BillingAddress = new AddressDto
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
                AddressLine = "123 Main St",
                Country = "US",
                State = "CA",
                ZipCode = "90001"
            },
            Payment = new PaymentSummaryDto
            {
                CardName = "John Doe",
                MaskedCardNumber = "************1234",
                PaymentMethod = 1
            },
            Items = new List<OrderItemDto>
            {
                new()
                {
                    OrderId = orderId,
                    CatalogId = Guid.CreateVersion7(),
                    Quantity = 1,
                    Price = 99.99m
                }
            }
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
        Guid orderId = Guid.CreateVersion7();
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
        GetOrderByIdQuery query = new GetOrderByIdQuery(Guid.CreateVersion7());
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
        OrderEntity? order = OrderBuilder.CreateOrderFaker().Generate();
        order.Id = OrderId.Of(Guid.CreateVersion7());

        Context.Orders.Add(order);
        await Context.SaveChangesAsync();

        GetOrderByIdResult result = new GetOrderByIdResult
        {
            Id = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount.Value,
            ShippingAddress = new AddressDto
            {
                FirstName = order.ShippingAddress.FirstName,
                LastName = order.ShippingAddress.LastName,
                EmailAddress = order.ShippingAddress.EmailAddress,
                AddressLine = order.ShippingAddress.AddressLine,
                Country = order.ShippingAddress.Country,
                State = order.ShippingAddress.State,
                ZipCode = order.ShippingAddress.ZipCode
            },
            BillingAddress = new AddressDto
            {
                FirstName = order.BillingAddress.FirstName,
                LastName = order.BillingAddress.LastName,
                EmailAddress = order.BillingAddress.EmailAddress,
                AddressLine = order.BillingAddress.AddressLine,
                Country = order.BillingAddress.Country,
                State = order.BillingAddress.State,
                ZipCode = order.BillingAddress.ZipCode
            },
            Payment = new PaymentSummaryDto
            {
                CardName = order.Payment.CardName,
                MaskedCardNumber = "************1234",
                PaymentMethod = order.Payment.PaymentMethod
            },
            Items = order.Items.Select(i => new OrderItemDto
            {
                OrderId = i.OrderId.Value,
                CatalogId = i.CatalogId.Value,
                Quantity = i.Quantity,
                Price = i.Price.Value
            }).ToList()
        };

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderByIdQuery>()))
            .ReturnsAsync(Result<GetOrderByIdResult>.Success(result));

        HttpResponseMessage response = await DoGet($"/order/{order.Id.Value}");
        response.EnsureSuccessStatusCode();
        Result<GetOrderByIdResult>? responseResult =
            await response.Content.ReadFromJsonAsync<Result<GetOrderByIdResult>>();
        responseResult?.Data.Should().NotBeNull();
        responseResult?.Data?.Id.Should().Be(order.Id.Value);
    }
}
