using BackendForFrontend.Api.Order.Dtos;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class DeleteOrderEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Order_Is_Deleted_Successfully()
    {
        Guid orderId = Guid.CreateVersion7();
        
        HttpResponseMessage response = await _client.DeleteAsync($"/api/order/{orderId}");
        response.EnsureSuccessStatusCode();
        
        Result<DeleteOrderResponse>? result = await response.Content.ReadFromJsonAsync<Result<DeleteOrderResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
