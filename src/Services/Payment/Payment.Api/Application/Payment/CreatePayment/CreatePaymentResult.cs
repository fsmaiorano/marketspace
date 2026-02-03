namespace Payment.Api.Application.Payment.CreatePayment;

public class CreatePaymentResult(Guid paymentId)
{
    public Guid PaymentId { get; init; } = paymentId;
}
