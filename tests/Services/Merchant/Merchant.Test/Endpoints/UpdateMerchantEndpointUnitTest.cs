using Merchant.Test.Fixtures;
using System.Net;

namespace Merchant.Test.Endpoints;

public class UpdateMerchantEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Updated_Successfully()
    {
        MerchantEntity? merchant = await fixture.CreateMerchant();

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
        Assert.True(response.StatusCode.Equals(HttpStatusCode.NoContent));

        Context.ChangeTracker.Clear();
        MerchantEntity? updatedMerchant = await Context.Merchants.FindAsync(merchant.Id);

        updatedMerchant.Should().NotBeNull();
        updatedMerchant.Name.Should().Be(command.Name);
    }
}