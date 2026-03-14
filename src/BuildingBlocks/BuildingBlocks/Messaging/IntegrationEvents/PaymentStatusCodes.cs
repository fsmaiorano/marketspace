namespace BuildingBlocks.Messaging.IntegrationEvents;

/// <summary>
/// Named constants for payment status integer values used in <see cref="PaymentStatusChangedIntegrationEvent"/>.
/// Using primitives in the event avoids tight enum coupling between services.
/// </summary>
public static class PaymentStatusCodes
{
    public const int Created = 1;
    public const int Processing = 2;
    public const int Authorized = 3;
    public const int Captured = 4;
    public const int Failed = 5;
    public const int Rejected = 6;
    public const int Cancelled = 7;
    public const int Refunded = 8;
    public const int Chargeback = 9;
}
