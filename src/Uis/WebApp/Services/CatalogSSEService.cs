using WebApp.Services.Abstractions;

namespace WebApp.Services;

/// <summary>
/// Catalog-specific SSE service implementation
/// Handles catalog operations like loading products, searching, and filtering
/// </summary>
public class CatalogSSEService(ILogger<CatalogSSEService> logger)
    : SSEServiceBase<CatalogRequest, CatalogResult>(logger)
{
    protected override async Task ExecuteOperationAsync(OperationStatus<CatalogResult> operation)
    {
        try
        {
            UpdateOperation(operation.Id, "Processing", 5, "Initializing catalog operation...");

            switch (operation.Type.ToLower())
            {
                case "catalog":
                    await ExecuteCatalogOperation(operation);
                    break;
                case "search":
                    await ExecuteSearchOperation(operation);
                    break;
                case "filter":
                    await ExecuteFilterOperation(operation);
                    break;
                default:
                    await ExecuteGenericOperation(operation);
                    break;
            }

            UpdateOperation(operation.Id, "Completed", 100, "Operation completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Operation {OperationId} failed", operation.Id);
            UpdateOperation(operation.Id, "Failed", operation.Progress, ex.Message);
        }
    }

    private async Task ExecuteCatalogOperation(OperationStatus<CatalogResult> operation)
    {
        var catalogItems = new List<ProductItem>();
        var totalItems = 50;

        UpdateOperation(operation.Id, "Processing", 10, "Connecting to catalog database...");
        await Task.Delay(800);

        UpdateOperation(operation.Id, "Processing", 20, "Loading product categories...");
        await Task.Delay(600);

        // Simulate loading products in batches
        for (int batch = 1; batch <= 5; batch++)
        {
            var progress = 20 + (batch * 15);
            UpdateOperation(operation.Id, "Processing", progress, $"Loading products batch {batch}/5...");

            // Create mock products for this batch
            for (int i = 1; i <= 10; i++)
            {
                var productId = (batch - 1) * 10 + i;
                catalogItems.Add(new ProductItem
                {
                    Id = productId,
                    Name = $"Product {productId}",
                    Price = Math.Round(19.99 + (productId * 5.50), 2),
                    Category = $"Category {(productId % 5) + 1}",
                    InStock = productId % 7 != 0,
                    Description = $"High-quality product {productId} with amazing features",
                    ImageUrl = $"/images/product-{productId}.jpg"
                });
            }

            // Update operation with partial results
            var partialResult = new CatalogResult
            {
                Items = catalogItems,
                TotalCount = catalogItems.Count,
                ExpectedTotal = totalItems
            };
            UpdateOperationResult(operation.Id, partialResult);
            
            await Task.Delay(700);
        }

        UpdateOperation(operation.Id, "Processing", 95, "Finalizing catalog data...");
        
        var finalResult = new CatalogResult
        {
            Items = catalogItems,
            TotalCount = catalogItems.Count,
            LoadTime = DateTime.UtcNow
        };
        UpdateOperationResult(operation.Id, finalResult);
        
        await Task.Delay(300);
    }

    private async Task ExecuteSearchOperation(OperationStatus<CatalogResult> operation)
    {
        var searchResults = new List<ProductItem>();
        
        UpdateOperation(operation.Id, "Processing", 15, "Parsing search query...");
        await Task.Delay(400);

        UpdateOperation(operation.Id, "Processing", 30, "Searching product database...");
        await Task.Delay(800);

        UpdateOperation(operation.Id, "Processing", 60, "Ranking search results...");
        
        // Mock search results
        for (int i = 1; i <= 15; i++)
        {
            searchResults.Add(new ProductItem
            {
                Id = i,
                Name = $"Search Result {i}",
                Price = Math.Round(15.99 + (i * 3.25), 2),
                Category = "Electronics",
                InStock = true,
                RelevanceScore = Math.Round(1.0 - (i * 0.05), 2)
            });
        }

        var result = new CatalogResult
        {
            Items = searchResults,
            TotalCount = searchResults.Count,
            SearchTime = 1.2
        };
        UpdateOperationResult(operation.Id, result);
        
        await Task.Delay(600);

        UpdateOperation(operation.Id, "Processing", 90, "Applying search filters...");
        await Task.Delay(400);
    }

    private async Task ExecuteFilterOperation(OperationStatus<CatalogResult> operation)
    {
        var filteredResults = new List<ProductItem>();
        
        UpdateOperation(operation.Id, "Processing", 25, "Applying price filters...");
        await Task.Delay(500);

        UpdateOperation(operation.Id, "Processing", 50, "Applying category filters...");
        await Task.Delay(600);

        UpdateOperation(operation.Id, "Processing", 75, "Sorting filtered results...");
        
        // Mock filtered results
        for (int i = 1; i <= 20; i++)
        {
            filteredResults.Add(new ProductItem
            {
                Id = i,
                Name = $"Filtered Product {i}",
                Price = Math.Round(25.99 + (i * 2.75), 2),
                Category = "Electronics",
                InStock = true,
                Rating = Math.Round(3.5 + (i % 3) * 0.5, 1)
            });
        }

        var result = new CatalogResult
        {
            Items = filteredResults,
            TotalCount = filteredResults.Count,
            FiltersApplied = 3
        };
        UpdateOperationResult(operation.Id, result);
        
        await Task.Delay(400);
    }

    private async Task ExecuteGenericOperation(OperationStatus<CatalogResult> operation)
    {
        for (int i = 1; i <= 8; i++)
        {
            await Task.Delay(400);
            var progress = 10 + (i * 10);
            UpdateOperation(operation.Id, "Processing", progress, $"Processing step {i}/8");
        }
    }
}

// Data models for catalog operations
public class CatalogRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public bool IncludeOutOfStock { get; set; } = true;
    public string? Query { get; set; }
    public string? SortBy { get; set; }
    public int MaxResults { get; set; } = 50;
    public string? Category { get; set; }
    public PriceRange? PriceRange { get; set; }
    public bool InStockOnly { get; set; }
}

public class PriceRange
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}

public class CatalogResult
{
    public List<ProductItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int? ExpectedTotal { get; set; }
    public DateTime? LoadTime { get; set; }
    public double? SearchTime { get; set; }
    public int? FiltersApplied { get; set; }
}

public class ProductItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool InStock { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public double? RelevanceScore { get; set; }
    public double? Rating { get; set; }
}