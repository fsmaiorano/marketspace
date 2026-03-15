using System.Text.Json.Serialization;

namespace Ai.Api.Application.Tools;

public class GetOrderStatusTool(HttpClient httpClient, IConfiguration configuration)
{
    private string BaseUrl => configuration["Services:OrderService:BaseUrl"]
        ?? throw new ArgumentNullException("Services:OrderService:BaseUrl is not configured");

    public async Task<string?> GetOrderStatusAsync(Guid orderId)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{BaseUrl}/order/{orderId}");

            if (!response.IsSuccessStatusCode)
                return null;

            OrderDto? order = await response.Content.ReadFromJsonAsync<OrderDto>();

            if (order is null)
                return null;

            return $"Order {order.Id} — Status: {order.Status}, Total: {order.TotalAmount:C}, Created: {order.CreatedAt:g}";
        }
        catch
        {
            return null;
        }
    }

    private record OrderDto(
        [property: JsonPropertyName("id")]          Guid Id,
        [property: JsonPropertyName("customerId")]  Guid CustomerId,
        [property: JsonPropertyName("status")]      string Status,
        [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
        [property: JsonPropertyName("createdAt")]   DateTimeOffset CreatedAt);
}
