using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Loggers.Abstractions;

public interface IApplicationLogger<out T> : ILogger<T>
{
}
