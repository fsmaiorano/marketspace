namespace BackendForFrontend.Api.Merchant.Dtos;

public class GetMerchantByIdResponse
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
}