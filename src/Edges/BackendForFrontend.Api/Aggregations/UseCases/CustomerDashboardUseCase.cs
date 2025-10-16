using BackendForFrontend.Api.Aggregations.Contracts;
using BackendForFrontend.Api.Aggregations.Dtos;
using BackendForFrontend.Api.Basket.Contracts;
using BackendForFrontend.Api.Order.Contracts;
using BackendForFrontend.Api.Catalog.Contracts;
using BuildingBlocks.Loggers.Abstractions;

namespace BackendForFrontend.Api.Aggregations.UseCases;

public class CustomerDashboardUseCase(
    IApplicationLogger<CustomerDashboardUseCase> applicationLogger,
    IBusinessLogger<CustomerDashboardUseCase> businessLogger,
    IBasketService basketService,
    IOrderService orderService,
    ICatalogService catalogService) : ICustomerDashboardUseCase
{
    public async Task<CustomerDashboardResponse> GetCustomerDashboardAsync(Guid customerId)
    {
        applicationLogger.LogInformation("Aggregating customer dashboard data for customer: {CustomerId}", customerId);

        try
        {
            // Aggregate data from multiple services
            var tasks = new List<Task>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        // Note: This would need proper customer-to-username mapping in real implementation
                        var basket = await basketService.GetBasketByIdAsync(customerId.ToString());
                        return new CustomerBasketSummary
                        {
                            Username = basket.Data.ShoppingCart.Username,
                            ItemCount = basket.Data.ShoppingCart.Items.Count,
                            TotalValue = basket.Data.ShoppingCart.Items.Sum(item => item.Price * item.Quantity)
                        };
                    }
                    catch (Exception ex)
                    {
                        applicationLogger.LogWarning(ex, "Failed to get basket for customer {CustomerId}", customerId);
                        return new CustomerBasketSummary();
                    }
                }),
                Task.Run(async () =>
                {
                    try
                    {
                        var orders = await orderService.GetOrdersByCustomerIdAsync(customerId);
                        return orders.Data?.Orders.Take(5).Select(order => new CustomerOrderSummary
                        {
                            Id = order.Id,
                            OrderName = order.OrderName,
                            Status = order.Status,
                            TotalAmount = order.TotalAmount,
                            CreatedAt = order.CreatedAt
                        }).ToList();
                    }
                    catch (Exception ex)
                    {
                        applicationLogger.LogWarning(ex, "Failed to get orders for customer {CustomerId}", customerId);
                        return new List<CustomerOrderSummary>();
                    }
                }),
                Task.Run(async () =>
                {
                    try
                    {
                        var catalog = await catalogService.GetCatalogListAsync(1, 10);
                        return catalog.Data?.Products.Take(3).Select(item => new RecommendedProduct
                        {
                            Id = item.Id, Name = item.Name, Price = item.Price, ImageFile = item.ImageUrl
                        }).ToList();
                    }
                    catch (Exception ex)
                    {
                        applicationLogger.LogWarning(ex, "Failed to get catalog recommendations");
                        return new List<RecommendedProduct>();
                    }
                })
            };

            await Task.WhenAll(tasks);

            var basketTask = (Task<CustomerBasketSummary>)tasks[0];
            var ordersTask = (Task<List<CustomerOrderSummary>>)tasks[1];
            var recommendationsTask = (Task<List<RecommendedProduct>>)tasks[2];

            var dashboard = new CustomerDashboardResponse
            {
                CustomerId = customerId,
                Basket = await basketTask,
                RecentOrders = await ordersTask,
                RecommendedProducts = await recommendationsTask
            };

            businessLogger.LogInformation("Successfully aggregated dashboard data for customer: {CustomerId}", customerId);
            return dashboard;
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "Failed to aggregate dashboard data for customer: {CustomerId}", customerId);
            throw;
        }
    }
}