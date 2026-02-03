namespace Payment.Api.Domain.Enums;

public enum PaymentTransactionTypeEnum
{
    Authorization = 1,
    Capture = 2,
    Refund = 3,
    Void = 4
}
