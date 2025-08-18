namespace Order.Api.Domain.ValueObjects;

public record Address
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;

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

    private static void ValidateInput(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be empty.", parameterName);
    }
}