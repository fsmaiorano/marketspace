namespace BuildingBlocks.Loggers;

public interface IApplicationLoggerFactory
{
    IApplicationLogger CreateLogger<T>();
    IApplicationLogger CreateLogger(string categoryName);
}