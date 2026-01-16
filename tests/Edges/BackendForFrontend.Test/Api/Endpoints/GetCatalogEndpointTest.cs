namespace BackendForFrontend.Test.Api.Endpoints;

public class GetCatalogEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();


    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Retrieved_Successfully()
    {
        CatalogApiFactory factory = new();
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        CatalogEntity? catalog = CatalogBuilder.CreateCatalogFaker().Generate();

        catalog.Id = CatalogId.Of(Guid.CreateVersion7());

        dbContext.Catalogs.Add(catalog);
        await dbContext.SaveChangesAsync();

        HttpResponseMessage response = await _client.GetAsync($"/api/catalog/{catalog.Id.Value}");
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
