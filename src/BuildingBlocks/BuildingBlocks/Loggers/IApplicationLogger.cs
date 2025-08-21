using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Loggers;

public interface IApplicationLogger : ILogger
{
    void LogTrace(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogCritical(string message, params object[] args);
    void LogCritical(Exception exception, string message, params object[] args);

    void LogTrace(string correlationId, string message, params object[] args);
    void LogDebug(string correlationId, string message, params object[] args);
    void LogInformation(string correlationId, string message, params object[] args);
    void LogWarning(string correlationId, string message, params object[] args);
    void LogError(string correlationId, string message, params object[] args);
    void LogError(string correlationId, Exception exception, string message, params object[] args);
    void LogCritical(string correlationId, string message, params object[] args);
    void LogCritical(string correlationId, Exception exception, string message, params object[] args);

    IApplicationLogger ForContext<T>();
    IApplicationLogger ForContext(string contextName);
}
