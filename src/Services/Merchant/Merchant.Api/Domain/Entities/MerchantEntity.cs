using Merchant.Api.Domain.Abstractions;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Domain.Entities;

public class MerchantEntity : Aggregate<MerchantId>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public Email Email { get; private set; } = null!;

    public new DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public static MerchantEntity Create(
        string name,
        string description,
        string address,
        string phoneNumber,
        Email email)
    {
        return new MerchantEntity
        {
            Id = MerchantId.Of(Guid.NewGuid()),
            Name = name,
            Description = description,
            Address = address,
            PhoneNumber = phoneNumber,
            Email = email,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}