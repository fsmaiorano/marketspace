using BackendForFrontend.Api.Aggregations.Dtos;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class CustomerDashboardEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Customer_Dashboard_Is_Retrieved_Successfully()
    {
        Guid customerId = Guid.NewGuid();
        
        HttpResponseMessage response = await _client.GetAsync($"/api/dashboard/{customerId}");
        response.EnsureSuccessStatusCode();
        
        Result<CustomerDashboardResponse>? result = await response.Content.ReadFromJsonAsync<Result<CustomerDashboardResponse>>();
        result?.Data?.CustomerId.Should().Be(customerId);
        result?.IsSuccess.Should().BeTrue();
    }
}
