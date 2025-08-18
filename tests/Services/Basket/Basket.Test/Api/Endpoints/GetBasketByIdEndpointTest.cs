using Builder;
using BuildingBlocks;
using Basket.Api.Application.Basket.GetBasketById;
using Basket.Api.Application.Dto;
using Basket.Api.Domain.Entities;
using Basket.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;

namespace Basket.Test.Api.Endpoints;

public class GetBasketByIdEndpointTest(BasketApiFactory factory) : IClassFixture<BasketApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IGetBasketByIdHandler> _mockHandler = new();
    private readonly BasketApiFactory _factory = factory;

    [Fact]
    public async Task Returns_Ok_When_Basket_Exists()
    {
        ShoppingCartDto dto = BasketBuilder.CreateShoppingCartDtoFaker().Generate();
        GetBasketByIdQuery query = new GetBasketByIdQuery(dto.Username);
        GetBasketByIdResult result = new GetBasketByIdResult(dto);

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetBasketByIdQuery>()))
            .ReturnsAsync(Result<GetBasketByIdResult>.Success(result));

        Result<GetBasketByIdResult> response = await _mockHandler.Object.HandleAsync(query);
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_NotFound_When_Basket_Does_Not_Exist()
    {
        ShoppingCartDto dto = BasketBuilder.CreateShoppingCartDtoFaker().Generate();
        GetBasketByIdQuery query = new GetBasketByIdQuery(dto.Username);
        Result<GetBasketByIdResult> result =
            Result<GetBasketByIdResult>.Failure($"Basket with username {query.Username} not found.");

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetBasketByIdQuery>()))
            .ReturnsAsync(result);

        Result<GetBasketByIdResult> response = await _mockHandler.Object.HandleAsync(query);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be($"Basket with username {query.Username} not found.");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        GetBasketByIdQuery query = new GetBasketByIdQuery("");
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetBasketByIdQuery>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(query);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Get_Basket_By_Id_Endpoint()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        BasketDbContext dbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();

        ShoppingCartEntity entity = BasketBuilder.CreateShoppingCartFaker().Generate();
        GetBasketByIdQuery query = new GetBasketByIdQuery(entity.Username);
        ShoppingCartDto dto = BasketBuilder.CreateShoppingCartDtoFaker().Generate();


        await dbContext.ShoppingCart.InsertOneAsync(entity);

        GetBasketByIdResult result = new(dto);

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetBasketByIdQuery>()))
            .ReturnsAsync(Result<GetBasketByIdResult>.Success(result));

        HttpRequestMessage request = new(HttpMethod.Get, $"/basket/{entity.Username}");
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Result<GetBasketByIdResult>? responseResult =
            await response.Content.ReadFromJsonAsync<Result<GetBasketByIdResult>>();
        responseResult?.Data.Should().NotBeNull();
    }
}