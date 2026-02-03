namespace Payment.Api.Domain.ValueObjects;

public class PaymentTransactionId
{
    public Guid Value { get; init; }

    private PaymentTransactionId(Guid value) => Value = value;

    public static PaymentTransactionId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PaymentTransactionId cannot be empty.", nameof(value));

        return new PaymentTransactionId(value);
    }
}
