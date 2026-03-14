namespace Catalog.Api.Domain.ValueObjects;

public record Stock(int Available, int Reserved = 0)
{
    public static Stock Of(int available, int reserved = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(available, nameof(available));
        ArgumentOutOfRangeException.ThrowIfNegative(reserved, nameof(reserved));
        return new Stock(available, reserved);
    }

    /// <summary>Moves <paramref name="quantity"/> from Available to Reserved (order placed).</summary>
    public Stock Reserve(int quantity)
    {
        if (quantity > Available)
            throw new InvalidOperationException(
                $"Insufficient stock. Available: {Available}, Requested: {quantity}.");
        return this with { Available = Available - quantity, Reserved = Reserved + quantity };
    }

    /// <summary>Removes <paramref name="quantity"/> from Reserved (payment confirmed).</summary>
    public Stock Confirm(int quantity)
    {
        if (quantity > Reserved)
            throw new InvalidOperationException(
                $"Cannot confirm more than reserved. Reserved: {Reserved}, Quantity: {quantity}.");
        return this with { Reserved = Reserved - quantity };
    }

    /// <summary>Moves <paramref name="quantity"/> from Reserved back to Available (payment failed/cancelled).</summary>
    public Stock Release(int quantity)
    {
        if (quantity > Reserved)
            throw new InvalidOperationException(
                $"Cannot release more than reserved. Reserved: {Reserved}, Quantity: {quantity}.");
        return this with { Available = Available + quantity, Reserved = Reserved - quantity };
    }

    /// <summary>Applies a manual delta to Available stock (merchant management).</summary>
    public Stock Apply(int delta)
    {
        int newAvailable = Available + delta;
        if (newAvailable < 0)
            throw new InvalidOperationException(
                $"Insufficient stock. Available: {Available}, Delta: {delta}.");
        return this with { Available = newAvailable };
    }
}