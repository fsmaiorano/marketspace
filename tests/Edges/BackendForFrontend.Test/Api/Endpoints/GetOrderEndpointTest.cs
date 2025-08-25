using BackendForFrontend.Api.Order.Dtos;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class GetOrderEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Order_Is_Retrieved_Successfully()
    {
        Guid orderId = Guid.NewGuid();
        
        HttpResponseMessage response = await _client.GetAsync($"/api/order/{orderId}");
        response.EnsureSuccessStatusCode();
        
        Result<GetOrderResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetOrderResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Returns_Ok_When_Orders_By_Customer_Are_Retrieved_Successfully()
    {
        Guid customerId = Guid.NewGuid();
        
        HttpResponseMessage response = await _client.GetAsync($"/api/order/customer/{customerId}");
        response.EnsureSuccessStatusCode();
        
        Result<GetOrderListResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetOrderListResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
