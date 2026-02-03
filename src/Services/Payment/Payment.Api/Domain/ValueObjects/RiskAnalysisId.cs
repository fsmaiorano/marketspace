namespace Payment.Api.Domain.ValueObjects;

public class RiskAnalysisId
{
    public Guid Value { get; init; }

    private RiskAnalysisId(Guid value) => Value = value;

    public static RiskAnalysisId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RiskAnalysisId cannot be empty.", nameof(value));

        return new RiskAnalysisId(value);
    }
}