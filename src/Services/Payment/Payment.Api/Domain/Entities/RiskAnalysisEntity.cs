using BuildingBlocks.Abstractions;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Payment.Api.Domain.Entities;

public class RiskAnalysisEntity : Entity<RiskAnalysisId>
{
    public PaymentId? PaymentId { get; private set; }

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

    private void ChangeIpAddress(string? ip)
    {
        if (IpAddress == ip)
            return;

        IpAddress = ip;
    }

    private void ChangeCountry(string? country)
    {
        if (Country == country)
            return;

        Country = country;
    }

    private void ChangeScore(int? score)
    {
        if (Score == score)
            return;

        Score = score;
    }

    private void ChangeDecision(RiskDecisionEnum decision)
    {
        if (Decision == decision)
            return;

        Decision = decision;
    }

    private void Touch() => LastModifiedAt = DateTime.UtcNow;

    public void Update(
        string? ipAddress = null,
        string? country = null,
        int? score = null,
        RiskDecisionEnum? decision = null)
    {
        if (ipAddress is not null)
            ChangeIpAddress(ipAddress);

        if (country is not null)
            ChangeCountry(country);

        if (score is not null)
            ChangeScore(score);

        if (decision is not null)
            ChangeDecision(decision.Value);

        Touch();
    }
}
