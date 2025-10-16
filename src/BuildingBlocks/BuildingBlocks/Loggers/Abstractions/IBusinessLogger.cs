using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Loggers.Abstractions;

public interface IBusinessLogger<out T> : ILogger<T>
{
}
