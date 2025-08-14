namespace Merchant.Api.Application.Merchant.GetMerchantById;

public record GetMerchantByIdQuery(Guid Id) 
{
    public Guid Id { get; init; } = Id;
}