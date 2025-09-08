using BackendForFrontend.Api.Basket.Dtos;
using Builder;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class GetBasketEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Retrieved_Successfully()
    {
        CreateBasketRequest createRequest = BackendForFrontendBuilder.CreateBasketRequestFaker();
        
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/basket", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        Result<CreateBasketResponse>? createResult = await createResponse.Content.ReadFromJsonAsync<Result<CreateBasketResponse>>();
        createResult?.IsSuccess.Should().BeTrue();
        string username = createResult!.Data!.ShoppingCart.Username;
        
        HttpResponseMessage response = await _client.GetAsync($"/api/basket/{username}");
        response.EnsureSuccessStatusCode();
        
        Result<GetBasketResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetBasketResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Retrieved_Empty_Successfully()
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/basket/nonexistentuser");
        response.EnsureSuccessStatusCode();
        
        Result<GetBasketResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetBasketResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
