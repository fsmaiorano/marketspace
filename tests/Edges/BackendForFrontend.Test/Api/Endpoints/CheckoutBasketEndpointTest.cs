using BackendForFrontend.Api.Basket.Dtos;
using Builder;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class CheckoutBasketEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Basket_Checkout_Is_Successful()
    {
        // Arrange: First create a basket
        CreateBasketRequest createBasketRequest = BackendForFrontendBuilder.CreateBasketRequestFaker();
        
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/basket", createBasketRequest);
        createResponse.EnsureSuccessStatusCode();
        
        Result<CreateBasketResponse>? createResult = await createResponse.Content.ReadFromJsonAsync<Result<CreateBasketResponse>>();
        createResult?.IsSuccess.Should().BeTrue();
        string username = createResult!.Data!.ShoppingCart.Username;
        
        // Act: Now checkout the basket using the same username
        CheckoutBasketRequest checkoutRequest = BackendForFrontendBuilder.CreateCheckoutBasketRequestFaker();
        checkoutRequest.Username = username; // Use the same username from the created basket
        
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/basket/checkout", checkoutRequest);
        response.EnsureSuccessStatusCode();
        
        // Assert
        Result<CheckoutBasketResponse>? result = await response.Content.ReadFromJsonAsync<Result<CheckoutBasketResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
