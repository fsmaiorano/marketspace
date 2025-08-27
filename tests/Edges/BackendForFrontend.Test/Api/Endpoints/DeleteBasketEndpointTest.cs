using BackendForFrontend.Api.Basket.Dtos;
using Builder;
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
        CreateBasketRequest createRequest = BackendForFrontendBuilder.CreateBasketRequestFaker();
        
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/basket", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        Result<CreateBasketResponse>? createResult = await createResponse.Content.ReadFromJsonAsync<Result<CreateBasketResponse>>();
        createResult?.IsSuccess.Should().BeTrue();
        string username = createResult!.Data!.Username;

        HttpResponseMessage response = await _client.DeleteAsync($"/api/basket/{username}");
        response.EnsureSuccessStatusCode();

        Result<DeleteBasketResponse>? result = await response.Content.ReadFromJsonAsync<Result<DeleteBasketResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}