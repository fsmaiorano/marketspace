using System.Collections.Concurrent;
using System.Threading.Channels;

namespace BackendForFrontend.Api.Services;

public record StockChangedEvent(Guid CatalogId, string ProductName, int NewStock, DateTimeOffset ChangedAt);

public interface IStockEventService
{
    ChannelReader<StockChangedEvent> Subscribe(string userId);
    void Unsubscribe(string userId);
    Task PublishAsync(string userId, StockChangedEvent evt);
}

public class StockEventService : IStockEventService
{
    private readonly ConcurrentDictionary<string, Channel<StockChangedEvent>> _channels = new();

    public ChannelReader<StockChangedEvent> Subscribe(string userId)
    {
        Channel<StockChangedEvent> channel = _channels.GetOrAdd(userId, _ => Channel.CreateUnbounded<StockChangedEvent>());
        return channel.Reader;
    }

    public void Unsubscribe(string userId)
    {
        if (_channels.TryRemove(userId, out Channel<StockChangedEvent>? channel))
            channel.Writer.TryComplete();
    }

    public async Task PublishAsync(string userId, StockChangedEvent evt)
    {
        if (_channels.TryGetValue(userId, out Channel<StockChangedEvent>? channel))
            await channel.Writer.WriteAsync(evt);
    }
}
