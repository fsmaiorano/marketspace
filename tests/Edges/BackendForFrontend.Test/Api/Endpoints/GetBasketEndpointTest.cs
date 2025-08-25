using BackendForFrontend.Api.Basket.Dtos;
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
        string username = "testuser";
        
        HttpResponseMessage response = await _client.GetAsync($"/api/basket/{username}");
        response.EnsureSuccessStatusCode();
        
        Result<GetBasketResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetBasketResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
