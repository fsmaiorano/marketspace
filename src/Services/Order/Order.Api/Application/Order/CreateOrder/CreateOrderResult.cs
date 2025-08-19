namespace Order.Api.Application.Order.CreateOrder;

public class CreateOrderResult(Guid orderId)
{
    public Guid OrderId { get; init; } = orderId;
}