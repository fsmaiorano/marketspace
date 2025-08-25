using BackendForFrontend.Api.Catalog.Dtos;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class GetCatalogEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Retrieved_Successfully()
    {
        Guid catalogId = Guid.NewGuid();
        
        HttpResponseMessage response = await _client.GetAsync($"/api/catalog/{catalogId}");
        response.EnsureSuccessStatusCode();
        
        Result<GetCatalogResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetCatalogResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Returns_Ok_When_Catalog_List_Is_Retrieved_Successfully()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/catalog");
        response.EnsureSuccessStatusCode();
        
        Result<GetCatalogListResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetCatalogListResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
