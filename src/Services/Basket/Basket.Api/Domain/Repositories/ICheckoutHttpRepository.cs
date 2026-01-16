using Basket.Api.Application.Basket.CheckoutBasket.Contracts;

namespace Basket.Api.Domain.Repositories;

public interface ICheckoutHttpRepository
{
    Task<CreateOrderResponse?> CreateOrderAsync(CreateOrderRequest request, string? idempotencyKey, string? correlationId);
}