namespace Catalog.Api.Application.Catalog.GetCatalogById;

public record GetCatalogByIdQuery(Guid Id) 
{
    public Guid Id { get; init; } = Id;
}