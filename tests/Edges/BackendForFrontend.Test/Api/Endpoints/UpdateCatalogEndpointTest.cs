using BackendForFrontend.Api.Catalog.Dtos;
using Builder;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class UpdateCatalogEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Updated_Successfully()
    {
        Guid catalogId = Guid.NewGuid();
        UpdateCatalogRequest request = BackendForFrontendBuilder.CreateUpdateCatalogRequestFaker();
        request.Id = catalogId;
        
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/catalog/{catalogId}", request);
        response.EnsureSuccessStatusCode();
        
        Result<UpdateCatalogResponse>? result = await response.Content.ReadFromJsonAsync<Result<UpdateCatalogResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}
