namespace Order.Api.Application.Dto;

public class OrderItemDto
{
    public Guid CatalogId { get; private set; } = Guid.Empty;
    public int Quantity { get; private set; } = 0;
    public decimal Price { get; private set; } = 0.0m;
}