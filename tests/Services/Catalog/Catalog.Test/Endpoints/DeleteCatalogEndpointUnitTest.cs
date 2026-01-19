using Catalog.Api.Domain.ValueObjects;
using Catalog.Test.Base;
using Catalog.Test.Fixtures;

namespace Catalog.Test.Endpoints;

public class DeleteCatalogEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{

    [Fact]
    public async Task Can_Delete_Catalog_Endpoint()
    {
        CatalogEntity? catalog = CatalogBuilder.CreateCatalogFaker().Generate();
        catalog.Id = CatalogId.Of(Guid.CreateVersion7());

        Context.Catalogs.Add(catalog);
        await Context.SaveChangesAsync();
        
        Context.Entry(catalog).State = EntityState.Detached;

        DeleteCatalogCommand command = CatalogBuilder.CreateDeleteCatalogCommandFaker(catalog.Id.Value).Generate();
        HttpRequestMessage request = new(HttpMethod.Delete, "/catalog") { Content = JsonContent.Create(command) };
        HttpResponseMessage response = await HttpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected success but got {response.StatusCode}. Error: {errorContent}");
        }
        
        response.EnsureSuccessStatusCode();
        Result<DeleteCatalogResult>? result = await response.Content.ReadFromJsonAsync<Result<DeleteCatalogResult>>();
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsSuccess.Should().BeTrue();
    }
}
