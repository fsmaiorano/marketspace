using Catalog.Test.Fixtures;

namespace Catalog.Test.Endpoints;

public class DeleteCatalogEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Delete_Catalog_Endpoint()
    {
        CatalogEntity catalog = await fixture.CreateCatalog();

        DeleteCatalogCommand command = CatalogBuilder.CreateDeleteCatalogCommandFaker(catalog.Id.Value).Generate();
        HttpResponseMessage response = await DoDelete($"/catalog/{command.Id}");
        Result<DeleteCatalogResult>? result = await response.Content.ReadFromJsonAsync<Result<DeleteCatalogResult>>();
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
}