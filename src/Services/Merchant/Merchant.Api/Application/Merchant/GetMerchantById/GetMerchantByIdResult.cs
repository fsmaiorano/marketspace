namespace Merchant.Api.Application.Merchant.GetMerchantById;

public class GetMerchantByIdResult()
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
}