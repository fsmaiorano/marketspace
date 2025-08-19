namespace Order.Api.Domain.ValueObjects;

public record Address
{
    private string FirstName { get; set; } = null!;
    private string LastName { get; set; } = null!;
    private string EmailAddress { get; set; } = null!;
    private string AddressLine { get; set; } = null!;
    private string Country { get; set; } = null!;
    private string State { get; set; } = null!;
    private string ZipCode { get; set; } = null!;

    protected Address()
    {
    }

    private Address(string firstName, string lastName, string emailAddress, string addressLine, string country,
        string state, string zipCode)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        AddressLine = addressLine;
        Country = country;
        State = state;
        ZipCode = zipCode;
    }

    public static Address Of(string firstName, string lastName, string emailAddress, string addressLine,
        string country, string state, string zipCode)
    {
        ValidateInput(firstName, nameof(firstName));
        ValidateInput(emailAddress, nameof(emailAddress));
        ValidateInput(addressLine, nameof(addressLine));
        ValidateInput(country, nameof(country));
        ValidateInput(state, nameof(state));
        ValidateInput(zipCode, nameof(zipCode));

        return new Address(firstName, lastName, emailAddress, addressLine, country, state, zipCode);
    }

    public static Address FromString(string addressString)
    {
        if (string.IsNullOrWhiteSpace(addressString))
            throw new ArgumentException("Address string cannot be empty.", nameof(addressString));

        string[] parts = addressString.Split('|');
        if (parts.Length != 7)
            throw new ArgumentException("Invalid address string format.", nameof(addressString));

        return new Address(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], parts[6]);
    }

    public override string ToString()
    {
        return $"{FirstName}|{LastName}|{EmailAddress}|{AddressLine}|{Country}|{State}|{ZipCode}";
    }

    private static void ValidateInput(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be empty.", parameterName);
    }
}