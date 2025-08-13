using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.CreateMerchant;

public class CreateMerchantCommand(string name, string description, string address, string phoneNumber, Email email)
{
    public string Name { get; private set; } = name;
    public string Description { get; private set; } = description;
    public string Address { get; private set; } = address;
    public string PhoneNumber { get; private set; } = phoneNumber;
    public Email Email { get; private set; } = email;
}