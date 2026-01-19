using Catalog.Test.Base;
using Catalog.Test.Fixtures;

namespace Catalog.Test.Endpoints;

public class CreateCatalogEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    private readonly Mock<ICreateCatalogHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Created_Successfully()
    {
        CreateCatalogCommand command = CatalogBuilder.CreateCreateCatalogCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/catalog", command);
        Result<CreateCatalogResult>? result = await response.Content.ReadFromJsonAsync<Result<CreateCatalogResult> >();
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}
