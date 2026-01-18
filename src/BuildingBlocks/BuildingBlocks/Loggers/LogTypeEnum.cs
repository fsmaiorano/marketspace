namespace BuildingBlocks.Loggers;

/// <summary>
/// Defines the type of log entry for categorization and filtering.
/// </summary>
public enum LogTypeEnum
{
    /// <summary>
    /// Technical/application-level logging (system operations, debugging, infrastructure).
    /// </summary>
    Application,
    
    /// <summary>
    /// Business/functional-level logging (business operations, user actions, workflows).
    /// </summary>
    Business,
    
    /// <summary>
    /// Exception logging (errors, failures, exception details).
    /// </summary>
    Exception
}
