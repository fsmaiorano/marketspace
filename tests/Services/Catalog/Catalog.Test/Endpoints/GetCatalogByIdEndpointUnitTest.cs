using Catalog.Api.Domain.ValueObjects;
using Catalog.Test.Base;
using Catalog.Test.Fixtures;

namespace Catalog.Test.Endpoints;

public class GetCatalogByIdEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Catalog_Exists()
    {
        CatalogEntity catalog = await fixture.CreateCatalog();
      
        using HttpResponseMessage response = await DoGet($"/catalog/{catalog.Id.Value}");
        Result<GetCatalogByIdResult>? result = await response.Content.ReadFromJsonAsync<Result<GetCatalogByIdResult>>();
        
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
}
