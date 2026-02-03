namespace Payment.Api.Application.Payment.UpdatePayment;

public class UpdatePaymentResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}
