using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Domain.Entities;

public class PaymentEntity : Aggregate<PaymentId>
{
    public Guid OrderId { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    public PaymentStatusEnum Status { get; private set; }
    public string? StatusDetail { get; private set; }

    public PaymentMethod Method { get; private set; }
    public string Provider { get; private set; }

    public string? ProviderTransactionId { get; private set; }
    public string? AuthorizationCode { get; private set; }

    // NAVIGATION
    public ICollection<PaymentAttemptEntity> Attempts { get; private set; } = new List<PaymentAttemptEntity>();

    public ICollection<PaymentTransactionEntity> Transactions { get; private set; } =
        new List<PaymentTransactionEntity>();

    public RiskAnalysisEntity? RiskAnalysis { get; private set; }

    public static PaymentEntity Create(Guid orderId, decimal amount, string currency, PaymentMethod method,
        string provider)
    {
        ArgumentNullException.ThrowIfNull(currency, nameof(currency));
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        ArgumentNullException.ThrowIfNull(provider, nameof(provider));

        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));

        PaymentEntity payment = new PaymentEntity
        {
            Id = PaymentId.Of(Guid.NewGuid()),
            OrderId = orderId,
            Amount = amount,
            Currency = currency,
            Method = method,
            Provider = provider,
            Status = PaymentStatusEnum.Created,
            CreatedAt = DateTime.UtcNow
        };

        return payment;
    }

    public void PatchStatus(PaymentStatusEnum status)
    {
        Status = status;
        Touch();
    }

    public void MarkProcessing()
    {
        Status = PaymentStatusEnum.Processing;
        Touch();
    }

    public void Approve(string transactionId, string? authCode)
    {
        Status = PaymentStatusEnum.Authorized;
        ProviderTransactionId = transactionId;
        AuthorizationCode = authCode;
        Touch();
    }

    public void Fail(string detail)
    {
        Status = PaymentStatusEnum.Failed;
        StatusDetail = detail;
        Touch();
    }

    public void AddAttempt(PaymentAttemptEntity attempt)
        => Attempts.Add(attempt);

    public void AddTransaction(PaymentTransactionEntity transaction)
        => Transactions.Add(transaction);

    public void SetRisk(RiskAnalysisEntity risk)
        => RiskAnalysis = risk;

    private void Touch() => LastModifiedAt = DateTime.UtcNow;
}