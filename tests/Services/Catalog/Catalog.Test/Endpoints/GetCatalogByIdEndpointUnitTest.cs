using Catalog.Api.Endpoints.Dtos;
using Catalog.Test.Fixtures;

namespace Catalog.Test.Endpoints;

public class GetCatalogByIdEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Catalog_Exists()
    {
        CatalogEntity catalog = await fixture.CreateCatalog();
      
        using HttpResponseMessage response = await DoGet($"/catalog/{catalog.Id.Value}");
        CatalogDto? result = await response.Content.ReadFromJsonAsync<CatalogDto>();
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(catalog.Id.Value);
        result.Name.Should().Be(catalog.Name);
        result.Description.Should().Be(catalog.Description);
    }
}
