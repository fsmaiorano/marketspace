namespace Basket.Api.Application.Basket.CheckoutBasket.Contracts;

public class CreateOrderResponse
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
}

