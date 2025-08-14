using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Merchant.CreateMerchant;

public class CreateMerchantCommand
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}