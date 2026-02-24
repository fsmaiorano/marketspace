using Catalog.Test.Fixtures;
using System.Net;

namespace Catalog.Test.Endpoints;

public class CreateCatalogEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Created_Successfully()
    {
        CreateCatalogCommand command = CatalogBuilder.CreateCreateCatalogCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/catalog", command);
        Assert.True(HttpStatusCode.Created.Equals(response.StatusCode));
    }
}