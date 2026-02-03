namespace Payment.Api.Application.Payment.DeletePayment;

public class DeletePaymentResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}
