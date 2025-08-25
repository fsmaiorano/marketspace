using BackendForFrontend.Api.Catalog.Dtos;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class DeleteCatalogEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Deleted_Successfully()
    {
        Guid catalogId = Guid.NewGuid();
        
        HttpResponseMessage response = await _client.DeleteAsync($"/api/catalog/{catalogId}");
        response.EnsureSuccessStatusCode();
        
        Result<DeleteCatalogResponse>? result = await response.Content.ReadFromJsonAsync<Result<DeleteCatalogResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
