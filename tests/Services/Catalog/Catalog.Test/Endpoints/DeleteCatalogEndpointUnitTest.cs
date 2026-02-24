using Catalog.Test.Fixtures;
using System.Net;

namespace Catalog.Test.Endpoints;

public class DeleteCatalogEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Delete_Catalog_Endpoint()
    {
        CatalogEntity catalog = await fixture.CreateCatalog();

        DeleteCatalogCommand command = CatalogBuilder.CreateDeleteCatalogCommandFaker(catalog.Id.Value).Generate();
        HttpResponseMessage response = await DoDelete($"/catalog/{command.Id}");
        Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));
    }
}