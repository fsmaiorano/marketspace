using BackendForFrontend.Api.Merchant.Dtos;
using Bogus;

namespace Builder;

public class BackendForFrontendBuilder
{
    public static CreateMerchantRequest CreateMerchantRequestFaker()
    {
        return new Faker<CreateMerchantRequest>().CustomInstantiator(f => new CreateMerchantRequest
        {
            Name = f.Company.CompanyName(),
            Description = f.Lorem.Sentence(),
            Address = f.Address.FullAddress(),
            PhoneNumber = f.Phone.PhoneNumber(),
            Email = f.Internet.Email()
        });
    }
}