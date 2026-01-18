namespace BuildingBlocks.Loggers;

/// <summary>
/// Unified logger interface using Serilog with structured logging and LogType categorization.
/// Supports Application, Business, and Exception logging types.
/// </summary>
/// <typeparam name="T">The type associated with the logger (typically the class using it)</typeparam>
public interface IAppLogger<T>
{
    /// <summary>
    /// Logs an informational message with specified log type.
    /// </summary>
    /// <param name="logTypeEnum">The type of log (Application, Business, Exception)</param>
    /// <param name="messageTemplate">Message template with placeholders</param>
    /// <param name="propertyValues">Values for the message template placeholders</param>
    void LogInformation(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues);

    /// <summary>
    /// Logs an informational message (defaults to Application log type).
    /// </summary>
    /// <param name="messageTemplate">Message template with placeholders</param>
    /// <param name="propertyValues">Values for the message template placeholders</param>
    void LogInformation(string messageTemplate, params object?[] propertyValues);

    /// <summary>
    /// Logs a warning message with specified log type.
    /// </summary>
    /// <param name="logTypeEnum">The type of log (Application, Business, Exception)</param>
    /// <param name="messageTemplate">Message template with placeholders</param>
    /// <param name="propertyValues">Values for the message template placeholders</param>
    void LogWarning(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues);

    /// <summary>
    /// Logs a warning message (defaults to Application log type).
    /// </summary>
    /// <param name="messageTemplate">Message template with placeholders</param>
    /// <param name="propertyValues">Values for the message template placeholders</param>
    void LogWarning(string messageTemplate, params object?[] propertyValues);

    /// <summary>
    /// Logs an error message with specified log type and optional exception.
    /// </summary>
    /// <param name="logTypeEnum">The type of log (Application, Business, Exception)</param>
    /// <param name="exception">The exception that occurred (optional)</param>
    /// <param name="messageTemplate">Message template with placeholders</param>
    /// <param name="propertyValues">Values for the message template placeholders</param>
    void LogError(LogTypeEnum logTypeEnum, Exception? exception, string messageTemplate, params object?[] propertyValues);

    /// <summary>
    /// Logs an error message with exception (defaults to Exception log type).
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="messageTemplate">Message template with placeholders</param>
    /// <param name="propertyValues">Values for the message template placeholders</param>
    void LogError(Exception exception, string messageTemplate, params object?[] propertyValues);

    /// <summary>
    /// Logs an error message without exception (defaults to Application log type).
    /// </summary>
    /// <param name="messageTemplate">Message template with placeholders</param>
    /// <param name="propertyValues">Values for the message template placeholders</param>
    void LogError(string messageTemplate, params object?[] propertyValues);

    /// <summary>
    /// Logs a debug message with specified log type.
    /// </summary>
    /// <param name="logTypeEnum">The type of log (Application, Business, Exception)</param>
    /// <param name="messageTemplate">Message template with placeholders</param>
    /// <param name="propertyValues">Values for the message template placeholders</param>
    void LogDebug(LogTypeEnum logTypeEnum, string messageTemplate, params object?[] propertyValues);

    /// <summary>
    /// Logs a debug message (defaults to Application log type).
    /// </summary>
    /// <param name="messageTemplate">Message template with placeholders</param>
    /// <param name="propertyValues">Values for the message template placeholders</param>
    void LogDebug(string messageTemplate, params object?[] propertyValues);
}