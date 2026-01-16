using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Application.Basket.DeleteBasket;
using Basket.Api.Application.Basket.GetBasketById;

namespace BackendForFrontend.Test.Mocks;

public class TestBasketService(
    HttpClient httpClient, 
    IApplicationLogger<TestBasketService> applicationLogger,
    IBusinessLogger<TestBasketService> businessLogger) : IBasketService
{
    public async Task<Result<CreateBasketResponse>> CreateBasketAsync(CreateBasketRequest request)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/basket", request);

            if (response.IsSuccessStatusCode)
            {
                Result<CreateBasketResult>? resultWrapper =
                    await response.Content.ReadFromJsonAsync<Result<CreateBasketResult>>();

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<CreateBasketResponse> basketResponse
                        = Result<CreateBasketResponse>.Success(new CreateBasketResponse
                        {
                            ShoppingCart = new CartDto()
                            {
                                Username = resultWrapper.Data.ShoppingCart.Username,
                                Items = resultWrapper.Data.ShoppingCart.Items.Select(item => new BasketItemDto
                                {
                                    ProductId = item.ProductId,
                                    ProductName = item.ProductName,
                                    Quantity = item.Quantity,
                                    Price = item.Price,
                                }).ToList()
                            }
                        });

                    applicationLogger.LogInformation("Basket created successfully: {@Basket}", basketResponse);
                    return basketResponse;
                }
            }

            applicationLogger.LogError("Failed to create basket. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating basket: {errorMessage}");
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "Error occurred while creating basket");
            throw;
        }
    }

    public async Task<Result<GetBasketResponse>> GetBasketByIdAsync(string username)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/basket/{username}");

            if (response.IsSuccessStatusCode)
            {
                Result<GetBasketByIdResult>? resultWrapper =
                    await response.Content.ReadFromJsonAsync<Result<GetBasketByIdResult>>();

                if (resultWrapper is { IsSuccess: true, Data: not null })
                {
                    Result<GetBasketResponse> basketResponse = Result<GetBasketResponse>.Success(new GetBasketResponse
                    {
                        ShoppingCart = new CartDto
                        {
                            Username = resultWrapper.Data.ShoppingCart.Username,
                            Items = resultWrapper.Data.ShoppingCart.Items.Select(item => new BasketItemDto
                            {
                                ProductId = item.ProductId,
                                ProductName = item.ProductName,
                                Quantity = item.Quantity,
                                Price = item.Price,
                            }).ToList()
                        }
                    });

                    applicationLogger.LogInformation("Basket retrieved successfully: {@Basket}", basketResponse);
                    return basketResponse;
                }
            }

            applicationLogger.LogError("Failed to retrieve basket. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving basket: {errorMessage}");
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "Error occurred while retrieving basket");
            throw;
        }
    }

    public async Task<Result<DeleteBasketResponse>> DeleteBasketAsync(string username)
    {
        try
        {
            DeleteBasketCommand command = BasketBuilder.CreateDeleteBasketCommandFaker(username).Generate();
            HttpRequestMessage request = new(HttpMethod.Delete, "/basket") { Content = JsonContent.Create(command) };

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                applicationLogger.LogInformation("Basket deleted successfully for user: {Username}", username);
                return Result<DeleteBasketResponse>.Success(new DeleteBasketResponse { IsSuccess = true });
            }

            applicationLogger.LogError("Failed to delete basket. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error deleting basket: {errorMessage}");
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "Error occurred while deleting basket");
            throw;
        }
    }

    public async Task<Result<CheckoutBasketResponse>> CheckoutBasketAsync(CheckoutBasketRequest request)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/basket/checkout", request);

            if (response.IsSuccessStatusCode)
            {
                applicationLogger.LogInformation("Basket checkout initiated successfully for user: {Username}", request.Username);
                return Result<CheckoutBasketResponse>.Success(new CheckoutBasketResponse { IsSuccess = true });
            }

            applicationLogger.LogError("Failed to checkout basket. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error checking out basket: {errorMessage}");
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "Error occurred while checking out basket");
            throw;
        }
    }
}