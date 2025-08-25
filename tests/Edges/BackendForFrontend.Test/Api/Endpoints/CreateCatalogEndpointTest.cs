using BackendForFrontend.Api.Catalog.Dtos;
using Builder;
using BuildingBlocks;
using FluentAssertions;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class CreateCatalogEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Created_Successfully()
    {
        CreateCatalogRequest request = BackendForFrontendBuilder.CreateCatalogRequestFaker();
        
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/catalog", request);
        response.EnsureSuccessStatusCode();
        
        Result<CreateCatalogResponse>? result = await response.Content.ReadFromJsonAsync<Result<CreateCatalogResponse>>();
        result?.Data?.Id.Should().NotBeEmpty();
        result?.IsSuccess.Should().BeTrue();
    }
}
