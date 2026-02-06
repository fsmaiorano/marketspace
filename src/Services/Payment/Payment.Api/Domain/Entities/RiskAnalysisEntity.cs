using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Payment.Api.Domain.Entities;

public class RiskAnalysisEntity : Entity<RiskAnalysisId>
{
    public PaymentId PaymentId { get; private set; }

    public string? IpAddress { get; private set; }
    public string? Country { get; private set; }
    public int? Score { get; private set; }
    public RiskDecisionEnum Decision { get; private set; }

    private RiskAnalysisEntity() { }

    [JsonConstructor]
    public RiskAnalysisEntity(RiskAnalysisId id, PaymentId paymentId, string? ipAddress, 
        string? country, int? score, RiskDecisionEnum decision)
    {
        Id = id;
        PaymentId = paymentId;
        IpAddress = ipAddress;
        Country = country;
        Score = score;
        Decision = decision;
    }

    public RiskAnalysisEntity(PaymentId paymentId, string? ip, string? country, int? score, RiskDecisionEnum decision)
    {
        PaymentId = paymentId;
        IpAddress = ip;
        Country = country;
        Score = score;
        Decision = decision;
    }
}
