namespace Order.Api.Domain.ValueObjects;

public record OrderItemId
{
    public Guid Value { get; set; }
    
    private OrderItemId(Guid value) => Value = value;
    
    public static OrderItemId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderItemId cannot be empty.", nameof(value));

        return new OrderItemId(value);
    }
}