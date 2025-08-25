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
        CheckoutBasketRequest request = BackendForFrontendBuilder.CreateCheckoutBasketRequestFaker();
        
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/basket/checkout", request);
        response.EnsureSuccessStatusCode();
        
        Result<CheckoutBasketResponse>? result = await response.Content.ReadFromJsonAsync<Result<CheckoutBasketResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
