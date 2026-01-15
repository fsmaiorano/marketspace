using BuildingBlocks.Abstractions;
using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Domain.Entities;

public class MerchantEntity : Aggregate<MerchantId>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;

    public new DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static MerchantEntity Create(
        string name,
        string description,
        string address,
        string phoneNumber,
        Email email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required.", nameof(address));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("PhoneNumber is required.", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(email.Value))
            throw new ArgumentException("E-mail is required.", nameof(phoneNumber));

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

    public void Update(
        string name,
        string description,
        string address,
        string phoneNumber,
        Email email)
    {
        Name = name;
        Description = description;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}