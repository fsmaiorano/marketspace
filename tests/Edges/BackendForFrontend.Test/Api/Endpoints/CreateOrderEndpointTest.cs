using BackendForFrontend.Api.Order.Dtos;
using Builder;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class CreateOrderEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Order_Is_Created_Successfully()
    {
        CreateOrderRequest request = BackendForFrontendBuilder.CreateOrderRequestFaker();
        
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/order", request);
        response.EnsureSuccessStatusCode();
        
        Result<CreateOrderResponse>? result = await response.Content.ReadFromJsonAsync<Result<CreateOrderResponse>>();
        result?.Data?.Id.Should().NotBeEmpty();
        result?.IsSuccess.Should().BeTrue();
    }
}
