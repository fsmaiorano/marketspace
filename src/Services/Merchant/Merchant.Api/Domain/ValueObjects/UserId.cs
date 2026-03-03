namespace Merchant.Api.Domain.ValueObjects;

public record UserId
{
    public Guid Value { get; init; }

    private UserId(Guid value) => Value = value;

    public static UserId Of(Guid value)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("UserId cannot be empty.", nameof(value))
            : new UserId(value);
    }
}