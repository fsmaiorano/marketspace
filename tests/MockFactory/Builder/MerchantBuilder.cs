using Bogus;
using Merchant.Api.Application.Merchant.CreateMerchant;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.ValueObjects;

namespace Builder;

public static class MerchantBuilder
{
    public static Faker<CreateMerchantCommand> CreateCreateMerchantCommandFaker(string email = "")
    {
        return new Faker<CreateMerchantCommand>()
            .RuleFor(m => m.Name, f => f.Company.CompanyName())
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.Address, f => f.Address.FullAddress())
            .RuleFor(m => m.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(m => m.Email,
                f => string.IsNullOrWhiteSpace(email) ? "marketspace@marketspace.com" : email);
    }

    public static Faker<MerchantEntity> CreateMerchantFaker(string email = "")
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