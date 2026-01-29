using Catalog.Test.Fixtures;

namespace Catalog.Test.Endpoints;

public class CreateCatalogEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    private readonly Mock<ICreateCatalogHandler> _mockHandler = new();
    private readonly TestFixture _fixture = fixture;

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Created_Successfully()
    {
        CreateCatalogCommand command = CatalogBuilder.CreateCreateCatalogCommandFaker().Generate();
        HttpResponseMessage response = await DoPost("/catalog", command);
        Result<CreateCatalogResult>? result = await response.Content.ReadFromJsonAsync<Result<CreateCatalogResult>>();
        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
    }
}