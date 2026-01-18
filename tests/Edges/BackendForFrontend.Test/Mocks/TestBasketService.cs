using Basket.Api.Application.Basket.CreateBasket;
using Basket.Api.Application.Basket.DeleteBasket;
using Basket.Api.Application.Basket.GetBasketById;

namespace BackendForFrontend.Test.Mocks;

public class TestBasketService(
    HttpClient httpClient, 
    IAppLogger<TestBasketService> logger) : IBasketService
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

                    return basketResponse;
                }
            }

            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating basket: {errorMessage}");
        }
        catch (Exception ex)
        {
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

                    return basketResponse;
                }
            }

            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error retrieving basket: {errorMessage}");
        }
        catch (Exception ex)
        {
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
                return Result<DeleteBasketResponse>.Success(new DeleteBasketResponse { IsSuccess = true });
            }

            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error deleting basket: {errorMessage}");
        }
        catch (Exception ex)
        {
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
                logger.LogInformation(LogTypeEnum.Application, "Basket checkout initiated successfully for user: {Username}", request.Username);
                return Result<CheckoutBasketResponse>.Success(new CheckoutBasketResponse { IsSuccess = true });
            }

            logger.LogError(LogTypeEnum.Application, null, "Failed to checkout basket. Status code: {StatusCode}", response.StatusCode);
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error checking out basket: {errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "Error occurred while checking out basket");
            throw;
        }
    }
}