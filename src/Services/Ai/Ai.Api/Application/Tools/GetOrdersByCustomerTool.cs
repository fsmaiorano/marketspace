using System.Text.Json.Serialization;

namespace Ai.Api.Application.Tools;

public class GetOrdersByCustomerTool(HttpClient httpClient, IConfiguration configuration)
{
    private string BaseUrl => configuration["Services:OrderService:BaseUrl"]
                              ?? throw new ArgumentNullException("Services:OrderService:BaseUrl is not configured");

    public async Task<string?> GetOrdersByCustomerAsync(Guid customerId, int limit = 5)
    {
        try
        {
            HttpResponseMessage response =
                await httpClient.GetAsync($"{BaseUrl}/order/customer/{customerId}?limit={limit}");

            if (!response.IsSuccessStatusCode)
                return null;

            List<OrderDto>? orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();

            if (orders is null || orders.Count == 0)
                return "No orders found for this customer.";

            IEnumerable<string> lines = orders.Select(o =>
                $"- Order {o.Id} | Status: {o.Status} | Total: {o.TotalAmount:C} | Created: {o.CreatedAt:g}");

            return $"Found {orders.Count} order(s):\n" + string.Join("\n", lines);
        }
        catch
        {
            return null;
        }
    }

    private record OrderDto(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("customerId")] Guid CustomerId,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
        [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt);
}
