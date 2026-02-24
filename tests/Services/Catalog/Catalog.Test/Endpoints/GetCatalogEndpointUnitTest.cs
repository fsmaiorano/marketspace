using BuildingBlocks.Pagination;
using Catalog.Api.Endpoints.Dtos;
using Catalog.Test.Fixtures;

namespace Catalog.Test.Endpoints;

public class GetCatalogEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Can_Get_Catalog_By_Id_Endpoint()
    {
        const int createCatalogCount = 5;
        for (int i = 0; i < createCatalogCount; i++)
            await fixture.CreateCatalog();

        await Context.SaveChangesAsync();

        HttpResponseMessage response = await DoGet("/catalog?PageIndex=1&PageSize=10");
        response.EnsureSuccessStatusCode();
        PaginatedResult<CatalogDto>? responseResult =
            await response.Content.ReadFromJsonAsync<PaginatedResult<CatalogDto>>();
        responseResult?.Data.Should().NotBeNull();
    }
}