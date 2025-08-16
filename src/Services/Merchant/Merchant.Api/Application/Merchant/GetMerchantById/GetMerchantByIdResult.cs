namespace Merchant.Api.Application.Merchant.GetMerchantById;

public class GetMerchantByIdResult()
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}