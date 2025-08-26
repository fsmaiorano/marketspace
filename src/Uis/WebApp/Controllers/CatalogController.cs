using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers.Abstractions;
using WebApp.Services;

namespace WebApp.Controllers;

/// <summary>
/// Catalog controller demonstrating the new generic SSE architecture
/// Inherits from SSEControllerBase to get all SSE functionality automatically
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CatalogController : SSEControllerBase<CatalogSSEService, CatalogRequest, CatalogResult>
{
    public CatalogController(CatalogSSEService catalogService, ILogger<CatalogController> logger) 
        : base(catalogService, logger)
    {
    }

    /// <summary>
    /// Catalog-specific endpoint with typed parameters
    /// </summary>
    [HttpPost("load")]
    public async Task<IActionResult> LoadCatalog([FromBody] CatalogLoadRequest request)
    {
        var parameters = new CatalogRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            IncludeOutOfStock = request.IncludeOutOfStock,
            Category = request.Category
        };

        var operationRequest = new OperationRequest<CatalogRequest>
        {
            OperationType = "catalog",
            Parameters = parameters
        };

        return await StartOperation(operationRequest);
    }

    /// <summary>
    /// Search-specific endpoint
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> SearchProducts([FromBody] SearchRequest request)
    {
        var parameters = new CatalogRequest
        {
            Query = request.Query,
            MaxResults = request.MaxResults,
            SortBy = request.SortBy,
            Category = request.Category
        };

        var operationRequest = new OperationRequest<CatalogRequest>
        {
            OperationType = "search",
            Parameters = parameters
        };

        return await StartOperation(operationRequest);
    }

    /// <summary>
    /// Filter-specific endpoint
    /// </summary>
    [HttpPost("filter")]
    public async Task<IActionResult> FilterProducts([FromBody] FilterRequest request)
    {
        var parameters = new CatalogRequest
        {
            Category = request.Category,
            PriceRange = request.PriceRange,
            InStockOnly = request.InStockOnly,
            PageSize = request.PageSize
        };

        var operationRequest = new OperationRequest<CatalogRequest>
        {
            OperationType = "filter",
            Parameters = parameters
        };

        return await StartOperation(operationRequest);
    }
}

// Request models for catalog operations
public class CatalogLoadRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public bool IncludeOutOfStock { get; set; } = true;
    public string? Category { get; set; }
}

public class SearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 50;
    public string? SortBy { get; set; }
    public string? Category { get; set; }
}

public class FilterRequest
{
    public string? Category { get; set; }
    public PriceRange? PriceRange { get; set; }
    public bool InStockOnly { get; set; }
    public int PageSize { get; set; } = 50;
}
