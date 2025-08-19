namespace Order.Api.Application.Dto;

public class OrderItemDto
{
    public Guid OrderId { get; set; } = Guid.Empty;
    public Guid CatalogId { get; set; } = Guid.Empty;
    public int Quantity { get; set; } = 0;
    public decimal Price { get; set; } = 0.0m;
}