using Merchant.Test.Fixtures;

namespace Merchant.Test.Endpoints;

public class UpdateMerchantEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Updated_Successfully()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker().Generate();
        Context.Merchants.Add(merchant);
        await Context.SaveChangesAsync();

        UpdateMerchantCommand command = new()
        {
            Id = merchant.Id.Value,
            Name = $"{merchant.Name}_Updated",
            Description = merchant.Description,
            Email = merchant.Email.Value,
            PhoneNumber = merchant.PhoneNumber,
            Address = merchant.Address
        };

        HttpResponseMessage response = await DoPut($"/merchant", command);
        Result<UpdateMerchantResult>? result = await response.Content.ReadFromJsonAsync<Result<UpdateMerchantResult>>();
        
        Context.ChangeTracker.Clear();
        MerchantEntity? updatedMerchant = await Context.Merchants.FindAsync(merchant.Id);

        result?.IsSuccess.Should().BeTrue();
        result?.Data.Should().NotBeNull();
        updatedMerchant.Should().NotBeNull();
        updatedMerchant.Name.Should().Be(command.Name);
    }
}