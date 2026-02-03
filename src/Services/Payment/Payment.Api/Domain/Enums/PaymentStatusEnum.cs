namespace Payment.Api.Domain.Enums;

public enum PaymentStatusEnum
{
    Created = 1,
    Processing = 2,
    Authorized = 3,
    Captured = 4,
    Failed = 5,
    Rejected = 6,
    Cancelled = 7,
    Refunded = 8,
    Chargeback = 9
}
