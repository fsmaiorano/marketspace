using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.ValueObjects;

namespace Payment.Api.Domain.Entities;

public class RiskAnalysisEntity : Entity<RiskAnalysisId>
{
    public PaymentId PaymentId { get; private set; }

    public string? IpAddress { get; private set; }
    public string? Country { get; private set; }
    public int? Score { get; private set; }
    public RiskDecisionEnum Decision { get; private set; }

    private RiskAnalysisEntity() { }

    public RiskAnalysisEntity(PaymentId paymentId, string? ip, string? country, int? score, RiskDecisionEnum decision)
    {
        PaymentId = paymentId;
        IpAddress = ip;
        Country = country;
        Score = score;
        Decision = decision;
    }
}
