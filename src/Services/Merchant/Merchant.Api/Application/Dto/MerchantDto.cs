using Merchant.Api.Domain.ValueObjects;

namespace Merchant.Api.Application.Dto;

public class MerchantDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public Email Email { get; set; } = null!;
}