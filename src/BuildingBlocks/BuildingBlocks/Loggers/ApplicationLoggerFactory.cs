using Microsoft.Extensions.Logging;
using BuildingBlocks.Services.Correlation;

namespace BuildingBlocks.Loggers;

public class ApplicationLoggerFactory(
    ILoggerFactory loggerFactory,
    ICorrelationIdService correlationIdService)
    : IApplicationLoggerFactory
{
    public IApplicationLogger CreateLogger<T>()
    {
        return CreateLogger(typeof(T).Name);
    }

    public IApplicationLogger CreateLogger(string categoryName)
    {
        ILogger logger = loggerFactory.CreateLogger(categoryName);
        return new ApplicationLogger(logger, correlationIdService, categoryName);
    }
}