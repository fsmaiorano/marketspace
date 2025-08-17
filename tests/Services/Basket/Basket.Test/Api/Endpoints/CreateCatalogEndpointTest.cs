using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Application.Dto;
using Builder;
using BuildingBlocks;
using FluentAssertions;
using Moq;
using System.Net.Http.Json;
using CreateBasketCommand = Basket.Api.Application.Basket.CreateBasket.CreateBasketCommand;

namespace Basket.Test.Api.Endpoints;

public class CreateBasketEndpointTest(BasketApiFactory factory) : IClassFixture<BasketApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<ICreateBasketHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Created_Successfully()
    {
        Guid merchantId = Guid.NewGuid();

        CreateBasketCommand command = BasketBuilder.CreateBasketCommandFaker().Generate();

        ShoppingCartDto cartDto = BasketBuilder.CreateShoppingCartDtoFaker().Generate();
        
        Result<CreateBasketResult>
            result = Result<CreateBasketResult>.Success(new CreateBasketResult(cartDto));

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateBasketCommand>()))
            .ReturnsAsync(result);

        Result<CreateBasketResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_Error_Result_When_Handler_Fails()
    {
        CreateBasketCommand command = BasketBuilder.CreateBasketCommandFaker().Generate();
        Result<CreateBasketResult> result = Result<CreateBasketResult>.Failure("Validation error");
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateBasketCommand>()))
            .ReturnsAsync(result);
        Result<CreateBasketResult> response = await _mockHandler.Object.HandleAsync(command);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Validation error");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        CreateBasketCommand command = BasketBuilder.CreateBasketCommandFaker().Generate();
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateBasketCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Create_Basket_Endpoint()
    {
        CreateBasketCommand command = BasketBuilder.CreateBasketCommandFaker().Generate();
        
        HttpResponseMessage response = await _client.PostAsJsonAsync("/basket", command);
        response.EnsureSuccessStatusCode();

        CreateBasketResult? result = await response.Content.ReadFromJsonAsync<CreateBasketResult>();
        result.Should().NotBeNull();
    }
}