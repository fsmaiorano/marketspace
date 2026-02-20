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
    
    private void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        
        if (Name == name)
            return;
        
        Name = name;
    }
    
    private void ChangeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (Description == description)
            return;

        Description = description;
    }

    private void ChangeAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required.", nameof(address));

        if (Address == address)
            return;

        Address = address;
    }

    private void ChangePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        if (PhoneNumber == phoneNumber)
            return;

        PhoneNumber = phoneNumber;
    }

    private void ChangeEmail(Email email)
    {
        if (Email == email)
            return;

        Email = email;
    }
    
    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    public void Update(
        string? name,
        string? description,
        string? address,
        string? phoneNumber,
        Email? email)
    {
        if (name is not null)
            ChangeName(name);

        if (description is not null)
            ChangeDescription(description);

        if (address is not null)
            ChangeAddress(address);

        if (phoneNumber is not null)
            ChangePhoneNumber(phoneNumber);

        if (email is not null)
            ChangeEmail(email);
        
        Touch();
    }
}