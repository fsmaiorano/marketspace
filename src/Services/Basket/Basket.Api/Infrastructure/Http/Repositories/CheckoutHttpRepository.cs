using Basket.Api.Application.Basket.CheckoutBasket.Contracts;
using Basket.Api.Domain.Repositories;
using BuildingBlocks.Loggers.Abstractions;

namespace Basket.Api.Infrastructure.Http.Repositories;

public class CheckoutHttpRepository(
    HttpClient httpClient,
    IConfiguration configuration,
    IApplicationLogger<CheckoutHttpRepository> applicationLogger) : ICheckoutHttpRepository
{
    public async Task<CreateOrderResponse?> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            string baseUrl = configuration["OrderService:BaseUrl"]
                             ?? throw new InvalidOperationException("OrderService BaseUrl is not configured.");

            applicationLogger.LogInformation("Calling Order Service to create order for customer: {CustomerId}",
                request.CustomerId);

            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{baseUrl}/order", request);

            if (response.IsSuccessStatusCode)
            {
                CreateOrderResponse? result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
                applicationLogger.LogInformation("Order created successfully: {OrderId}", result?.OrderId);
                return result;
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            applicationLogger.LogError("Failed to create order. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);
            return null;
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "Exception occurred while calling Order Service for customer: {CustomerId}",
                request.CustomerId);
            throw;
        }
    }
}