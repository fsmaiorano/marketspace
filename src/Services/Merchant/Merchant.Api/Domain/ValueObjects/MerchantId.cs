namespace Merchant.Api.Domain.ValueObjects;

public record MerchantId
{
    public Guid Value { get; init; }

    private MerchantId(Guid value) => Value = value;

    public static MerchantId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MerchantId cannot be empty.", nameof(value));

        return new MerchantId(value);
    }
}