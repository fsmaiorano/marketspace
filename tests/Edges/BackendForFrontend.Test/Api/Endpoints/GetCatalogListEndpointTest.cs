using BackendForFrontend.Api.Catalog.Dtos;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class GetCatalogListEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    
    [Fact]
    public async Task Returns_Ok_When_Catalog_List_Is_Retrieved_Successfully()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/catalog?pageIndex=1&pageSize=10");
        response.EnsureSuccessStatusCode();
        
        Result<GetCatalogListResponse>? result = await response.Content.ReadFromJsonAsync<Result<GetCatalogListResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}