using BackendForFrontend.Api.Catalog.Dtos;
using BackendForFrontend.Api.Catalog.UseCases;
using BackendForFrontend.Api.Merchant.UseCases;
using BackendForFrontend.Api.MerchantDashboard.Dtos;
using BackendForFrontend.Api.Order.Dtos;
using BackendForFrontend.Api.Order.UseCases;
using BackendForFrontend.Api.Services;
using BuildingBlocks;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Channels;

namespace BackendForFrontend.Api.MerchantDashboard;

public static class MerchantDashboardEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/merchant-dashboard/overview",
                async (ClaimsPrincipal user, MerchantUseCase merchantUseCase, CatalogUseCase catalogUseCase,
                    OrderUseCase orderUseCase) =>
                {
                    string? userId = GetUserId(user);
                    if (string.IsNullOrEmpty(userId))
                        return Results.Unauthorized();

                    Result<BackendForFrontend.Api.Merchant.Dtos.GetMerchantMeResponse> merchantResult =
                        await merchantUseCase.GetMerchantMeAsync(userId);

                    if (!merchantResult.IsSuccess || merchantResult.Data is null)
                        return Results.BadRequest(merchantResult.Error);

                    Result<BackendForFrontend.Api.Catalog.Dtos.GetCatalogListResponse> catalogResult =
                        await catalogUseCase.GetCatalogByMerchantIdAsync(merchantResult.Data.Id, 1, 100);

                    if (!catalogResult.IsSuccess || catalogResult.Data is null)
                        return Results.BadRequest(catalogResult.Error);

                    List<CatalogDto> products = catalogResult.Data.Products;
                    Dictionary<Guid, CatalogDto> productLookup = products.ToDictionary(product => product.Id);

                    Result<GetOrdersByCatalogIdsResponse> orderResult = await orderUseCase.GetOrdersByCatalogIdsAsync(
                        products.Select(product => product.Id),
                        100);

                    if (!orderResult.IsSuccess || orderResult.Data is null)
                        return Results.BadRequest(orderResult.Error);

                    List<MerchantDashboardOrderDto> merchantOrders = orderResult.Data.Orders
                        .Select(order =>
                        {
                            List<MerchantDashboardOrderItemDto> items = order.Items
                                .Where(item => productLookup.ContainsKey(item.ProductId))
                                .Select(item =>
                                {
                                    CatalogDto product = productLookup[item.ProductId];
                                    return new MerchantDashboardOrderItemDto
                                    {
                                        CatalogId = item.ProductId,
                                        ProductName = product.Name,
                                        Quantity = item.Quantity,
                                        UnitPrice = item.Price,
                                        LineTotal = item.Price * item.Quantity
                                    };
                                })
                                .ToList();

                            return new MerchantDashboardOrderDto
                            {
                                OrderId = order.Id,
                                CustomerEmail = order.ShippingAddress.EmailAddress
                                    ?? order.BillingAddress.EmailAddress
                                    ?? $"Customer {order.CustomerId}",
                                Status = order.Status,
                                MerchantTotalAmount = items.Sum(item => item.LineTotal),
                                CreatedAt = order.CreatedAt,
                                Items = items
                            };
                        })
                        .Where(order => order.Items.Count > 0)
                        .OrderByDescending(order => order.CreatedAt)
                        .ToList();

                    MerchantDashboardOverviewResponse overview = new()
                    {
                        Summary = new MerchantDashboardSummaryDto
                        {
                            TotalOrders = merchantOrders.Count,
                            TotalUnitsSold = merchantOrders.Sum(order => order.Items.Sum(item => item.Quantity)),
                            TotalRevenue = merchantOrders.Sum(order => order.MerchantTotalAmount),
                            ProcessingOrders = merchantOrders.Count(order =>
                                order.Status is "Created" or "Processing" or "ReadyForDelivery" or "DeliveryInProgress"),
                            CompletedOrders = merchantOrders.Count(order =>
                                order.Status is "Completed" or "Delivered" or "Finalized")
                        },
                        RecentOrders = merchantOrders.Take(10).ToList(),
                        ProductSales = products
                            .Select(product =>
                            {
                                List<MerchantDashboardOrderItemDto> soldItems = merchantOrders
                                    .SelectMany(order => order.Items)
                                    .Where(item => item.CatalogId == product.Id)
                                    .ToList();

                                return new MerchantDashboardProductSalesDto
                                {
                                    CatalogId = product.Id,
                                    ProductName = product.Name,
                                    UnitsSold = soldItems.Sum(item => item.Quantity),
                                    OrderCount = merchantOrders.Count(order => order.Items.Any(item => item.CatalogId == product.Id)),
                                    CurrentStock = product.Stock
                                };
                            })
                            .OrderByDescending(product => product.UnitsSold)
                            .ThenBy(product => product.ProductName)
                            .Take(10)
                            .ToList(),
                        GeneratedAt = DateTimeOffset.UtcNow
                    };

                    return Results.Ok(Result<MerchantDashboardOverviewResponse>.Success(overview));
                })
            .RequireAuthorization()
            .WithName("MerchantDashboardOverview")
            .WithTags("MerchantDashboard")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        app.MapGet("/api/merchant-dashboard/stream",
                async (HttpContext context, ClaimsPrincipal user, IStockEventService stockEventService) =>
                {
                    string? userId = GetUserId(user);
                    if (string.IsNullOrEmpty(userId))
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }

                    // Disable response buffering so SSE data is flushed immediately
                    context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpResponseBodyFeature>()?.DisableBuffering();

                    context.Response.ContentType = "text/event-stream";
                    context.Response.Headers.CacheControl = "no-cache";
                    context.Response.Headers.Connection = "keep-alive";
                    context.Response.Headers.Append("X-Accel-Buffering", "no");

                    ChannelReader<StockChangedEvent> reader = stockEventService.Subscribe(userId);
                    try
                    {
                        await context.Response.WriteAsync("data: {\"type\":\"connected\"}\n\n");
                        await context.Response.Body.FlushAsync();

                        using PeriodicTimer heartbeat = new(TimeSpan.FromSeconds(25));
                        Task heartbeatTask = Task.Run(async () =>
                        {
                            while (await heartbeat.WaitForNextTickAsync(context.RequestAborted))
                            {
                                await context.Response.WriteAsync(": heartbeat\n\n");
                                await context.Response.Body.FlushAsync();
                            }
                        }, context.RequestAborted);

                        await foreach (StockChangedEvent evt in reader.ReadAllAsync(context.RequestAborted))
                        {
                            string json = JsonSerializer.Serialize(evt,
                                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                            await context.Response.WriteAsync($"data: {json}\n\n");
                            await context.Response.Body.FlushAsync();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Client disconnected
                    }
                    finally
                    {
                        stockEventService.Unsubscribe(userId);
                    }
                })
            .RequireAuthorization()
            .WithName("MerchantDashboardStream")
            .WithTags("MerchantDashboard");
    }

    private static string? GetUserId(ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? user.FindFirst("sub")?.Value;
}
