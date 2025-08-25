using BackendForFrontend.Api.Basket.Dtos;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class DeleteBasketEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Basket_Is_Deleted_Successfully()
    {
        string username = "testuser";
        
        HttpResponseMessage response = await _client.DeleteAsync($"/api/basket/{username}");
        response.EnsureSuccessStatusCode();
        
        Result<DeleteBasketResponse>? result = await response.Content.ReadFromJsonAsync<Result<DeleteBasketResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
