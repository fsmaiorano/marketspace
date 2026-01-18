using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BuildingBlocks.Loggers;

/// <summary>
/// Unified logger implementation using Serilog with structured logging.
/// Automatically adds LogType as a structured property for filtering and categorization.
/// </summary>
/// <typeparam name="T">The type associated with the logger</typeparam>
public sealed class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public AppLogger(ILogger<T> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogInformation(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues)
    {
        using (LogContext.PushProperty("LogType", logTypeEnum.ToString()))
        {
            _logger.LogInformation(messageTemplate, propertyValues);
        }
    }

    public void LogInformation(string messageTemplate, params object?[] propertyValues)
    {
        LogInformation(LogTypeEnum.Application, messageTemplate, propertyValues);
    }

    public void LogWarning(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues)
    {
        using (LogContext.PushProperty("LogType", logTypeEnum.ToString()))
        {
            _logger.LogWarning(messageTemplate, propertyValues);
        }
    }

    public void LogWarning(string messageTemplate, params object?[] propertyValues)
    {
        LogWarning(LogTypeEnum.Application, messageTemplate, propertyValues);
    }

    public void LogError(LogTypeEnum logTypeEnum, Exception? exception, string messageTemplate, params object?[] propertyValues)
    {
        using (LogContext.PushProperty("LogType", logTypeEnum.ToString()))
        {
            if (exception != null)
            {
                _logger.LogError(exception, messageTemplate, propertyValues);
            }
            else
            {
                _logger.LogError(messageTemplate, propertyValues);
            }
        }
    }

    public void LogError(Exception exception, string messageTemplate, params object?[] propertyValues)
    {
        LogError(LogTypeEnum.Exception, exception, messageTemplate, propertyValues);
    }

    public void LogError(string messageTemplate, params object?[] propertyValues)
    {
        LogError(LogTypeEnum.Application, null, messageTemplate, propertyValues);
    }

    public void LogDebug(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues)
    {
        using (LogContext.PushProperty("LogType", logTypeEnum.ToString()))
        {
            _logger.LogDebug(messageTemplate, propertyValues);
        }
    }

    public void LogDebug(string messageTemplate, params object?[] propertyValues)
    {
        LogDebug(LogTypeEnum.Application, messageTemplate, propertyValues);
    }
}