using BuildingBlocks.Loggers.Abstractions;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BuildingBlocks.Loggers;

public class ApplicationLogger<T>(ILogger<T> logger) : IApplicationLogger<T>
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logger.IsEnabled(logLevel);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return logger.BeginScope(state);
    }

    public void LogTrace(string message, params object[] args)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.LogTrace(message, args);
        }
    }

    public void LogDebug(string message, params object[] args)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.LogDebug(message, args);
        }
    }

    public void LogInformation(string message, params object[] args)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.LogInformation(message, args);
        }
    }

    public void LogWarning(string message, params object[] args)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.LogWarning(message, args);
        }
    }

    public void LogWarning(Exception exception, string message, params object[] args)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.LogWarning(exception, message, args);
        }
    }

    public void LogError(string message, params object[] args)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.LogError(message, args);
        }
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.LogError(exception, message, args);
        }
    }

    public void LogCritical(string message, params object[] args)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.LogCritical(message, args);
        }
    }

    public void LogCritical(Exception exception, string message, params object[] args)
    {
        using (LogContext.PushProperty("LogType", "application"))
        {
            logger.LogCritical(exception, message, args);
        }
    }
}
