namespace Payment.Api.Domain.ValueObjects;

public class PaymentAttemptId
{
    public Guid Value { get; set; }

    private PaymentAttemptId(Guid value) => Value = value;

    public static PaymentAttemptId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PaymentId cannot be empty.", nameof(value));

        return new PaymentAttemptId(value);
    }
}
