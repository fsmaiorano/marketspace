namespace BackendForFrontend.Api.Merchant.Dtos;

public class GetMerchantByIdResponse
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}