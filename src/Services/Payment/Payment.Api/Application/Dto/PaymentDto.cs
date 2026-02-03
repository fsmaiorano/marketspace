namespace Payment.Api.Application.Dto;

public class PaymentDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? ProviderTransactionId { get; init; }
    public string? AuthorizationCode { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastModifiedAt { get; init; }
}
