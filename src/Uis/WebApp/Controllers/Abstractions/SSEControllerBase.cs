using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Abstractions;

namespace WebApp.Controllers.Abstractions;

/// <summary>
/// Abstract base controller for SSE operations
/// Provides common SSE endpoints that can be used by any controller
/// </summary>
/// <typeparam name="TService">The SSE service type</typeparam>
/// <typeparam name="TRequest">Type of operation request</typeparam>
/// <typeparam name="TResult">Type of operation result</typeparam>
public abstract class SSEControllerBase<TService, TRequest, TResult> : Controller
    where TService : SSEServiceBase<TRequest, TResult>
{
    protected readonly TService SSEService;
    protected readonly ILogger Logger;

    protected SSEControllerBase(TService sseService, ILogger logger)
    {
        SSEService = sseService;
        Logger = logger;
    }

    /// <summary>
    /// Generic endpoint to start any SSE operation
    /// </summary>
    [HttpPost("start-operation")]
    public virtual async Task<IActionResult> StartOperation([FromBody] OperationRequest<TRequest> request)
    {
        try
        {
            string operationId = await SSEService.StartOperationAsync(request.OperationType, request.Parameters);

            return Json(new
            {
                success = true,
                operationId,
                message = "Operation started successfully",
                sseUrl = Url.Action("StreamStatus", new { operationId })
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start {OperationType} operation", request.OperationType);
            return Json(new { success = false, message = "Failed to start operation" });
        }
    }

    /// <summary>
    /// Generic SSE streaming endpoint
    /// </summary>
    [HttpGet("stream/{operationId}")]
    public virtual async Task StreamStatus(string operationId)
    {
        await SSEService.StreamOperationUpdatesAsync(Response, operationId);
    }

    /// <summary>
    /// Get operation status as JSON (for polling alternative)
    /// </summary>
    [HttpGet("status/{operationId}")]
    public virtual IActionResult GetOperationStatus(string operationId)
    {
        OperationStatus<TResult>? operation = SSEService.GetOperationStatus(operationId);

        if (operation == null)
        {
            return NotFound(new { message = "Operation not found" });
        }

        return Json(new
        {
            operationId = operation.Id,
            status = operation.Status,
            progress = operation.Progress,
            message = operation.Message,
            startTime = operation.StartTime,
            lastUpdated = operation.LastUpdated,
            isComplete = operation.Status == "Completed" || operation.Status == "Failed",
            result = operation.Result
        });
    }

    /// <summary>
    /// Get all active operations (for monitoring)
    /// </summary>
    [HttpGet("operations/active")]
    public virtual IActionResult GetActiveOperations()
    {
        var operations = SSEService.GetActiveOperations()
            .Select(o => new
            {
                id = o.Id,
                type = o.Type,
                status = o.Status,
                progress = o.Progress,
                message = o.Message,
                startTime = o.StartTime,
                lastUpdated = o.LastUpdated
            });

        return Json(operations);
    }
}

/// <summary>
/// Generic request model for starting operations
/// </summary>
public class OperationRequest<TParameters>
{
    public string OperationType { get; set; } = string.Empty;
    public TParameters? Parameters { get; set; }
}