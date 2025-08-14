namespace Merchant.Api.Application.Merchant.GetMerchantById;

public class GetMerchantByIdResult(Guid id, string name, string email, string phoneNumber, string address)
{
    public Guid Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Email { get; set; } = email;
    public string PhoneNumber { get; set; } = phoneNumber;
    public string Address { get; set; } = address;
}