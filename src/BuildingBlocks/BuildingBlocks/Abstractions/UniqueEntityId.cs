using BuildingBlocks.Exceptions;

namespace BuildingBlocks.Abstractions;

public sealed record UniqueEntityId
{
    private readonly Guid _value;

    public UniqueEntityId(string? value = null)
    {
        _value = value is null
            ? GenerateNewGuid()
            : Guid.TryParse(value, out var guidValue)
                ? guidValue
                : throw new ArgumentException("Invalid GUID format.", nameof(value));
    }

    private UniqueEntityId(Guid value)
    {
        _value = value;
    }

    public override string ToString() => _value.ToString();

    public Guid ToGuid() => _value;

    /// <summary>
    /// Creates a new UniqueEntityId from a string value.
    /// </summary>
    public static UniqueEntityId Of(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new DomainException("The UniqueEntityId value cannot be null or empty.")
            : new UniqueEntityId(value);
    }

    /// <summary>
    /// Creates a new UniqueEntityId from a Guid value.
    /// </summary>
    public static UniqueEntityId Of(Guid value)
    {
        return new UniqueEntityId(value);
    }

    /// <summary>
    /// Creates a new UniqueEntityId with a new Guid.
    /// </summary>
    public static UniqueEntityId NewId()
    {
        return new UniqueEntityId(GenerateNewGuid());
    }

    #region Equality Members

    public bool Equals(UniqueEntityId? other)
    {
        if (other is null) return false;
        return ReferenceEquals(this, other) || _value.Equals(other._value);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    #endregion

    #region Conversion Operators

    /// <summary>
    /// Implicit conversion from UniqueEntityId to Guid.
    /// </summary>
    public static implicit operator Guid(UniqueEntityId id) => id._value;

    /// <summary>
    /// Explicit conversion from Guid to UniqueEntityId.
    /// </summary>
    public static explicit operator UniqueEntityId(Guid value) => new(value);

    /// <summary>
    /// Explicit conversion from string to UniqueEntityId.
    /// </summary>
    public static explicit operator UniqueEntityId(string value) => Of(value);

    #endregion

    private static Guid GenerateNewGuid() => Guid.CreateVersion7();
}