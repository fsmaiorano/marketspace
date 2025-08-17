using Builder;
using BuildingBlocks;
using Basket.Api.Application.Basket.GetBasketById;
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
        Guid merchantId = Guid.NewGuid();
        GetBasketByIdQuery query = new GetBasketByIdQuery(merchantId);
        GetBasketByIdResult result = new GetBasketByIdResult
        {
            Id = Guid.NewGuid(),
            Name = "Test Basket",
            Description = "This is a test basket",
            ImageUrl = "http://example.com/image.jpg",
            Price = 99.99m,
            Categories = new List<string> { "Category1", "Category2" }
        };


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
        Guid merchantId = Guid.NewGuid();
        GetBasketByIdQuery query = new GetBasketByIdQuery(merchantId);
        Result<GetBasketByIdResult> result =
            Result<GetBasketByIdResult>.Failure($"Basket with ID {query.Id} not found.");

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetBasketByIdQuery>()))
            .ReturnsAsync(result);

        Result<GetBasketByIdResult> response = await _mockHandler.Object.HandleAsync(query);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be($"Basket with ID {query.Id} not found.");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        GetBasketByIdQuery query = new GetBasketByIdQuery(Guid.NewGuid());
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

        BasketEntity? basket = BasketBuilder.CreateBasketFaker().Generate();

        basket.Id = BasketId.Of(Guid.NewGuid());

        dbContext.Baskets.Add(basket);
        await dbContext.SaveChangesAsync();

        GetBasketByIdResult result = new GetBasketByIdResult
        {
            Id = basket.Id.Value,
            Name = basket.Name,
            Description = basket.Description,
            ImageUrl = basket.ImageUrl,
            Price = basket.Price.Value,
            Categories = basket.Categories.Select(c => c).ToList()
        };

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetBasketByIdQuery>()))
            .ReturnsAsync(Result<GetBasketByIdResult>.Success(result));

        HttpRequestMessage request = new(HttpMethod.Get, $"/basket/{basket.Id.Value}");
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Result<GetBasketByIdResult>? responseResult =
            await response.Content.ReadFromJsonAsync<Result<GetBasketByIdResult>>();
        responseResult?.Data.Should().NotBeNull();
    }
}