using Basket.Api.Application.Dto;
using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;

namespace Basket.Api.Application.Basket.CreateBasket;

public sealed class CreateBasketHandler(
    IBasketRepository repository, 
    IApplicationLogger<CreateBasketHandler> applicationLogger,
    IBusinessLogger<CreateBasketHandler> businessLogger)
    : ICreateBasketHandler
{
    public async Task<Result<CreateBasketResult>> HandleAsync(CreateBasketCommand command)
    {
        try
        {
            applicationLogger.LogInformation("Processing create basket request for user: {Username}", command.Username);
            
            ShoppingCartEntity cartEntity = new ShoppingCartEntity
            {
                Username = command.Username,
                Items = command.Items.Select(item => new ShoppingCartItemEntity
                {
                    ProductName = item.ProductName,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity, 
                    Price = item.Price,
                }).ToList(),
            };

            ShoppingCartEntity result = await repository.CreateCartAsync(cartEntity);

            businessLogger.LogInformation("Basket created successfully. Username: {Username}, ItemCount: {ItemCount}", 
                cartEntity.Username, 
                cartEntity.Items.Count);

            ShoppingCartDto cartDto = new ShoppingCartDto()
            {
                Username = result.Username,
                Items = result.Items.Select(item => new ShoppingCartItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity, 
                    Price = item.Price
                }).ToList()
            };

            return Result<CreateBasketResult>.Success(new CreateBasketResult(cartDto));
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while creating the basket: {Command}", command);
            return Result<CreateBasketResult>.Failure($"An error occurred while creating the basket: {ex.Message}");
        }
    }
}