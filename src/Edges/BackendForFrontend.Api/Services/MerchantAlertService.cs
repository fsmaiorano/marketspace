using System.Collections.Concurrent;
using System.Threading.Channels;

namespace BackendForFrontend.Api.Services;

public record MerchantAlertEvent(
    Guid OrderId,
    Guid CatalogId,
    string ProductName,
    int RequestedQuantity,
    int AvailableQuantity,
    string Reason,
    DateTimeOffset OccurredAt);

public interface IMerchantAlertService
{
    ChannelReader<MerchantAlertEvent> Subscribe(string userId);
    void Unsubscribe(string userId);
    Task PublishAsync(string userId, MerchantAlertEvent alert);
}

public class MerchantAlertService : IMerchantAlertService
{
    private readonly ConcurrentDictionary<string, Channel<MerchantAlertEvent>> _channels = new();

    public ChannelReader<MerchantAlertEvent> Subscribe(string userId)
    {
        Channel<MerchantAlertEvent> channel =
            _channels.GetOrAdd(userId, _ => Channel.CreateUnbounded<MerchantAlertEvent>());
        return channel.Reader;
    }

    public void Unsubscribe(string userId)
    {
        if (_channels.TryRemove(userId, out Channel<MerchantAlertEvent>? channel))
            channel.Writer.TryComplete();
    }

    public async Task PublishAsync(string userId, MerchantAlertEvent alert)
    {
        if (_channels.TryGetValue(userId, out Channel<MerchantAlertEvent>? channel))
            await channel.Writer.WriteAsync(alert);
    }
}
