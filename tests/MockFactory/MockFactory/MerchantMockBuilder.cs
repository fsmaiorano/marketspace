using Bogus;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.ValueObjects;

namespace MockFactory;

public static class MerchantMockBuilder
{
    public static Faker<MerchantEntity> CreateMerchantFaker(string email)
    {
        return new Faker<MerchantEntity>()
            .RuleFor(m => m.Id, f => MerchantId.Of(f.Random.Guid()))
            .RuleFor(m => m.Name, f => f.Company.CompanyName())
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.Address, f => f.Address.FullAddress())
            .RuleFor(m => m.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(m => m.Email,
                f => string.IsNullOrWhiteSpace(email) ? Email.Of(f.Internet.Email()) : Email.Of(email));
    }
}