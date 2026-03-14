namespace BuildingBlocks.Messaging.IntegrationEvents;

public class PaymentData
{
    public string CardNumber { get; set; } = null!;
    public string CardName { get; set; } = null!;
    public string Expiration { get; set; } = null!;
    public string Cvv { get; set; } = null!;
    public int PaymentMethod { get; set; }
}
