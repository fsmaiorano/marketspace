namespace Order.Api.Domain.ValueObjects;

public record OrderId
{
    public Guid Value { get; set; }
    
    private OrderId(Guid value) => Value = value;
    
    public static OrderId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(value));

        return new OrderId(value);
    }
}