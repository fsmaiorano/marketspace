namespace BackendForFrontend.Test.Api.Endpoints;


public class DeleteCatalogEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Deleted_Successfully()
    {
        CreateCatalogRequest createRequest = BackendForFrontendBuilder.CreateCatalogRequestFaker();
    
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/catalog", createRequest);
        createResponse.EnsureSuccessStatusCode();
    
        Result<CreateCatalogResponse>? createResult = await createResponse.Content.ReadFromJsonAsync<Result<CreateCatalogResponse>>();
        createResult?.IsSuccess.Should().BeTrue();
        Guid catalogId = createResult!.Data!.Id;

        HttpResponseMessage response = await _client.DeleteAsync($"/api/catalog/{catalogId}");
        response.EnsureSuccessStatusCode();

        Result<DeleteCatalogResponse>? result = await response.Content.ReadFromJsonAsync<Result<DeleteCatalogResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }

}