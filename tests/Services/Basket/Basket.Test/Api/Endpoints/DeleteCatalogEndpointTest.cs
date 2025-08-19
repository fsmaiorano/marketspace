using Basket.Api.Application.Basket.CreateBasket;
using Builder;
using BuildingBlocks;
using Basket.Api.Application.Basket.DeleteBasket;
using Basket.Api.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Moq;
using System.Net.Http.Json;

namespace Basket.Test.Api.Endpoints;

public class DeleteCatalogEndpointTest(BasketApiFactory factory) : IClassFixture<BasketApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IDeleteBasketHandler> _mockHandler = new();
    private readonly BasketApiFactory _factory = factory;

    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Deleted_Successfully()
    {
        Guid catalogId = Guid.NewGuid();

        DeleteBasketCommand command = BasketBuilder.CreateDeleteBasketCommandFaker().Generate();
        Result<DeleteBasketResult> result = Result<DeleteBasketResult>.Success(new DeleteBasketResult(true));

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteBasketCommand>()))
            .ReturnsAsync(result);

        Result<DeleteBasketResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_Error_Result_When_Handler_Fails()
    {
        DeleteBasketCommand command = BasketBuilder.CreateDeleteBasketCommandFaker().Generate();
        Result<DeleteBasketResult> result = Result<DeleteBasketResult>.Failure("Validation error");
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteBasketCommand>()))
            .ReturnsAsync(result);
        Result<DeleteBasketResult> response = await _mockHandler.Object.HandleAsync(command);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Validation error");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        DeleteBasketCommand command = BasketBuilder.CreateDeleteBasketCommandFaker().Generate();
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteBasketCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Delete_Basket_Endpoint()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IMongoClient client = scope.ServiceProvider.GetRequiredService<IMongoClient>();

        ShoppingCartEntity? shoppingCart = BasketBuilder.CreateShoppingCartFaker().Generate();

        await client.GetDatabase("BasketInMemoryDbForTesting")
            .GetCollection<ShoppingCartEntity>("ShoppingCart")
            .InsertOneAsync(shoppingCart);

        HttpRequestMessage request = new(HttpMethod.Delete, "/basket") { Content = JsonContent.Create(shoppingCart) };
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Result<DeleteBasketResult>? result = await response.Content.ReadFromJsonAsync<Result<DeleteBasketResult>>();
        result?.IsSuccess.Should().BeTrue();
    }
}