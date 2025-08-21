using Microsoft.Extensions.Logging;
using BuildingBlocks.Services.Correlation;

namespace BuildingBlocks.Loggers;

public class ApplicationLogger(ILogger logger, ICorrelationIdService correlationIdService, string categoryName)
    : IApplicationLogger
{
    public ApplicationLogger(ILogger<ApplicationLogger> logger, ICorrelationIdService correlationIdService)
        : this(logger, correlationIdService, nameof(ApplicationLogger))
    {
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => logger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) where TState : notnull
    {
        logger.Log(logLevel, eventId, state, exception, formatter);
    }

    public void LogTrace(string message, params object[] args) =>
        LogTrace(correlationIdService.GetCorrelationId(), message, args);

    public void LogDebug(string message, params object[] args) =>
        LogDebug(correlationIdService.GetCorrelationId(), message, args);

    public void LogInformation(string message, params object[] args) =>
        LogInformation(correlationIdService.GetCorrelationId(), message, args);

    public void LogWarning(string message, params object[] args) =>
        LogWarning(correlationIdService.GetCorrelationId(), message, args);

    public void LogError(string message, params object[] args) =>
        LogError(correlationIdService.GetCorrelationId(), message, args);

    public void LogError(Exception exception, string message, params object[] args) =>
        LogError(correlationIdService.GetCorrelationId(), exception, message, args);

    public void LogCritical(string message, params object[] args) =>
        LogCritical(correlationIdService.GetCorrelationId(), message, args);

    public void LogCritical(Exception exception, string message, params object[] args) =>
        LogCritical(correlationIdService.GetCorrelationId(), exception, message, args);

    public void LogTrace(string correlationId, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Trace))
            logger.LogTrace("[{CorrelationId}] [{CategoryName}] " + message,
                PrependIds(correlationId, args));
    }

    public void LogDebug(string correlationId, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("[{CorrelationId}] [{CategoryName}] " + message,
                PrependIds(correlationId, args));
    }

    public void LogInformation(string correlationId, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[{CorrelationId}] [{CategoryName}] " + message,
                PrependIds(correlationId, args));
    }

    public void LogWarning(string correlationId, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("[{CorrelationId}] [{CategoryName}] " + message,
                PrependIds(correlationId, args));
    }

    public void LogError(string correlationId, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Error))
            logger.LogError("[{CorrelationId}] [{CategoryName}] " + message,
                PrependIds(correlationId, args));
    }

    public void LogError(string correlationId, Exception exception, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Error))
            logger.LogError(exception, "[{CorrelationId}] [{CategoryName}] " + message,
                PrependIds(correlationId, args));
    }

    public void LogCritical(string correlationId, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Critical))
            logger.LogCritical("[{CorrelationId}] [{CategoryName}] " + message,
                PrependIds(correlationId, args));
    }

    public void LogCritical(string correlationId, Exception exception, string message, params object[] args)
    {
        if (logger.IsEnabled(LogLevel.Critical))
            logger.LogCritical(exception, "[{CorrelationId}] [{CategoryName}] " + message,
                PrependIds(correlationId, args));
    }

    public IApplicationLogger ForContext<T>() => ForContext(typeof(T).Name);

    public IApplicationLogger ForContext(string contextName) =>
        new ApplicationLogger(logger, correlationIdService, contextName);

    private object[] PrependIds(string correlationId, object[] args)
    {
        object[] result = new object[args.Length + 2];
        result[0] = correlationId;
        result[1] = categoryName;
        Array.Copy(args, 0, result, 2, args.Length);
        return result;
    }
}