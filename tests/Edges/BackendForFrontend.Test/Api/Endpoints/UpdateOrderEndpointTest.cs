using BackendForFrontend.Api.Order.Dtos;
using Builder;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class UpdateOrderEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Order_Is_Updated_Successfully()
    {
        Guid orderId = Guid.CreateVersion7();
        UpdateOrderRequest request = BackendForFrontendBuilder.CreateUpdateOrderRequestFaker();
        request.Id = orderId;
        
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/order/{orderId}", request);
        response.EnsureSuccessStatusCode();
        
        Result<UpdateOrderResponse>? result = await response.Content.ReadFromJsonAsync<Result<UpdateOrderResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
