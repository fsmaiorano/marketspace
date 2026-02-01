using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using BuildingBlocks.Messaging.DomainEvents.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Basket.Api.Infrastructure.Data.Repositories;

public class BasketDataRepository(BasketDbContext context, IDomainEventDispatcher eventDispatcher) : IBasketDataRepository
{
    public async Task<ShoppingCartEntity> CreateCartAsync(ShoppingCartEntity cart)
    {
        await DeleteCartAsync(cart.Username);

        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        return cart;
    }

    public async Task<ShoppingCartEntity?> GetCartAsync(string username)
    {
        return await context.ShoppingCarts
            .FirstOrDefaultAsync(sc => sc.Username == username);
    }

    public async Task DeleteCartAsync(string username)
    {
        ShoppingCartEntity? cart = await context.ShoppingCarts
            .FirstOrDefaultAsync(c => c.Username == username);

        if (cart != null)
        {
            context.ShoppingCarts.Remove(cart);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckoutAsync(string username)
    {
        try
        {
            ShoppingCartEntity? cart = await context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.Username == username);

            if (cart == null)
                return true;

            cart.Checkout(cart);
            await eventDispatcher.DispatchAsync(cart.DomainEvents, CancellationToken.None);
            cart.ClearDomainEvents();

            context.ShoppingCarts.Remove(cart);
            await context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}