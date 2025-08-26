using System.Collections.Concurrent;
using System.Text.Json;

namespace WebApp.Services.Abstractions;

/// <summary>
/// Abstract base class for Server-Sent Events operations
/// Provides generic SSE functionality that can be extended by specific services
/// </summary>
/// <typeparam name="TRequest">Type of request/input parameters for operations</typeparam>
/// <typeparam name="TResult">Type of operation result</typeparam>
public abstract class SSEServiceBase<TRequest, TResult>
{
    protected readonly ILogger Logger;
    private readonly ConcurrentDictionary<string, OperationStatus<TResult>> _operations = new();

    protected SSEServiceBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Starts a new async operation and returns operation ID
    /// </summary>
    /// <param name="operationType">Type of operation to perform</param>
    /// <param name="request">Request data/parameters for the operation</param>
    public virtual async Task<string> StartOperationAsync(string operationType, TRequest? request = default)
    {
        string operationId = Guid.NewGuid().ToString();
        OperationStatus<TResult> operation = new OperationStatus<TResult>
        {
            Id = operationId,
            Type = operationType,
            Status = "Started",
            Progress = 0,
            StartTime = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
            Request = request
        };

        _operations.TryAdd(operationId, operation);

        // Start operation in background
        _ = Task.Run(async () => await ExecuteOperationAsync(operation));

        return await Task.FromResult(operationId);
    }

    /// <summary>
    /// Gets the current status of an operation
    /// </summary>
    public OperationStatus<TResult>? GetOperationStatus(string operationId)
    {
        return _operations.GetValueOrDefault(operationId);
    }

    /// <summary>
    /// Removes an operation from tracking
    /// </summary>
    public void CleanupOperation(string operationId)
    {
        _operations.TryRemove(operationId, out _);
    }

    /// <summary>
    /// Writes SSE message to HTTP response
    /// </summary>
    public async Task WriteSSEMessageAsync(HttpResponse response, string eventType, object data)
    {
        string json = JsonSerializer.Serialize(data);
        await response.WriteAsync($"event: {eventType}\n");
        await response.WriteAsync($"data: {json}\n\n");
        await response.Body.FlushAsync();
    }

    /// <summary>
    /// Streams operation updates via SSE until completion
    /// </summary>
    public async Task StreamOperationUpdatesAsync(HttpResponse response, string operationId)
    {
        // Set SSE headers
        response.Headers["Content-Type"] = "text/event-stream";
        response.Headers["Cache-Control"] = "no-cache";
        response.Headers["Connection"] = "keep-alive";
        response.Headers["Access-Control-Allow-Origin"] = "*";

        OperationStatus<TResult>? operation = GetOperationStatus(operationId);
        if (operation == null)
        {
            await WriteSSEMessageAsync(response, "error", new { message = "Operation not found" });
            return;
        }

        // Send initial status
        await WriteSSEMessageAsync(response, "status", operation);

        // Stream updates until complete
        while (operation?.Status == "Processing" || operation?.Status == "Started")
        {
            await Task.Delay(500); // Check every 500ms
            operation = GetOperationStatus(operationId);

            if (operation != null)
            {
                await WriteSSEMessageAsync(response, "status", operation);
            }
        }

        // Send final status and close
        if (operation != null)
        {
            await WriteSSEMessageAsync(response, "complete", operation);
        }

        // Cleanup operation after streaming
        CleanupOperation(operationId);
    }

    /// <summary>
    /// Abstract method to be implemented by derived services
    /// Defines the actual work to be performed for each operation type
    /// </summary>
    protected abstract Task ExecuteOperationAsync(OperationStatus<TResult> operation);

    /// <summary>
    /// Updates operation status, progress, and message
    /// </summary>
    protected void UpdateOperation(string operationId, string status, int progress, string? message = null)
    {
        if (!_operations.TryGetValue(operationId, out var operation))
            return;

        operation.Status = status;
        operation.Progress = progress;
        operation.Message = message;
        operation.LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates operation result
    /// </summary>
    protected void UpdateOperationResult(string operationId, TResult result)
    {
        if (!_operations.TryGetValue(operationId, out var operation))
            return;

        operation.Result = result;
        operation.LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets all active operations (for monitoring/debugging)
    /// </summary>
    public IEnumerable<OperationStatus<TResult>> GetActiveOperations()
    {
        return _operations.Values
            .Where(o => o.Status != "Completed" && o.Status != "Failed")
            .OrderByDescending(o => o.StartTime);
    }

    /// <summary>
    /// Background cleanup of old operations
    /// </summary>
    public async Task CleanupOldOperationsAsync(TimeSpan maxAge)
    {
        DateTime cutoff = DateTime.UtcNow.Subtract(maxAge);
        List<string> toRemove = _operations
            .Where(kvp => kvp.Value.Status is "Completed" or "Failed"
                          && kvp.Value.LastUpdated < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();
        
        /// Streams operation updates via SSE until completion
        foreach (string operationId in toRemove)
        {
            _operations.TryRemove(operationId, out _);
            Logger.LogInformation("Cleaned up old operation {OperationId}", operationId);
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Generic operation status class
/// </summary>
/// <typeparam name="TResult">Type of the operation result</typeparam>
public class OperationStatus<TResult>
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string? Message { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LastUpdated { get; set; }
    public object? Request { get; set; }
    public TResult? Result { get; set; }
}