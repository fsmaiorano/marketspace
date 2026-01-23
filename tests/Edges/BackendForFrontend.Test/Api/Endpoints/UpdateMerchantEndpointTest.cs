namespace BackendForFrontend.Test.Api.Endpoints;

public class UpdateMerchantEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly BackendForFrontendFactory _factory = factory;

    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Updated_Successfully()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        MerchantDbContext dbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();

        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker().Generate();

        dbContext.Merchants.Add(merchant);
        await dbContext.SaveChangesAsync();

        UpdateMerchantRequest updateRequest = new()

        {
            Id = merchant.Id.Value,
            Name = "Updated Catalog Name",
            Description = "Updated Description",
            Email = merchant.Email.Value,
            Address = merchant.Address,
            PhoneNumber = merchant.PhoneNumber
        };

        HttpResponseMessage updateResponse =
            await _client.PutAsJsonAsync($"/api/merchant/{merchant.Id.Value}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        Result<UpdateMerchantResponse>? updateResult =
            await updateResponse.Content.ReadFromJsonAsync<Result<UpdateMerchantResponse>>();
        updateResult?.IsSuccess.Should().BeTrue();
    }
}