using System.Net.Http.Json;
using Basket.Api.Application.Basket.CheckoutBasket.Contracts;
using Basket.Api.Domain.Repositories;

namespace MarketSpace.TestFixtures.Mocks;

public class TestCheckoutHttpRepository : ICheckoutHttpRepository
{
    private readonly HttpClient _orderApiClient;

    public TestCheckoutHttpRepository(HttpClient orderApiClient)
    {
        _orderApiClient = orderApiClient;
    }

    public async Task<CreateOrderResponse?> CreateOrderAsync(CreateOrderRequest request, string? idempotencyKey, string? correlationId)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/order")
        {
            Content = JsonContent.Create(request)
        };

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            message.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);

        if (!string.IsNullOrWhiteSpace(correlationId))
            message.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);

        HttpResponseMessage response = await _orderApiClient.SendAsync(message);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
    }
}
