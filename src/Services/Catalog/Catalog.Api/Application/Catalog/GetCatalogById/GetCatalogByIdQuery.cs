namespace Merchant.Api.Application.Merchant.GetMerchantById;

public record GetCatalogByIdQuery(Guid Id) 
{
    public Guid Id { get; init; } = Id;
}