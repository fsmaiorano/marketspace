namespace Payment.Api.Domain.ValueObjects;

public record PaymentId
{
    public Guid Value { get; set; }

    private PaymentId(Guid value) => Value = value;

    public static PaymentId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PaymentId cannot be empty.", nameof(value));

        return new PaymentId(value);
    }
}