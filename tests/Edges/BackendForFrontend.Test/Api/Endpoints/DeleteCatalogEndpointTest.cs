namespace BackendForFrontend.Test.Api.Endpoints;

using CatalogTestFixture = Catalog.Test.Fixtures.TestFixture;

public class DeleteCatalogEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Deleted_Successfully()
    {
        CatalogTestFixture catalogFixture = new();
        using IServiceScope scope = catalogFixture.Services.CreateScope();
        CatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        CatalogEntity? catalog = CatalogBuilder.CreateCatalogFaker().Generate();

        catalog.Id = CatalogId.Of(Guid.CreateVersion7());

        dbContext.Catalogs.Add(catalog);
        await dbContext.SaveChangesAsync();


        HttpResponseMessage response = await _client.DeleteAsync($"/api/catalog/{catalog.Id.Value}");
        response.EnsureSuccessStatusCode();

        Result<DeleteCatalogResponse>? result =
            await response.Content.ReadFromJsonAsync<Result<DeleteCatalogResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}