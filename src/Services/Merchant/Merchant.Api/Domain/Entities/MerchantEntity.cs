using Merchant.Api.Domain.Abstractions;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Domain.Entities;

public class MerchantEntity : Aggregate<MerchantId>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public Email Email { get; set; } = null!;

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
            Name = name,
            Description = description,
            Address = address,
            PhoneNumber = phoneNumber,
            Email = email,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}