using System.Collections.Concurrent;

namespace BackendForFrontend.Api.Services;

public interface IMerchantUserMappingService
{
    void Register(string merchantId, string userId);
    void Unregister(string merchantId);
    bool TryGetUserId(string merchantId, out string? userId);
}

/// <summary>
/// Tracks which userId (JWT sub) corresponds to each merchantId so that
/// RabbitMQ subscribers — which know merchantId from catalog entities but have no
/// active HTTP context — can look up the right SSE channel.
/// Populated when a merchant connects to the SSE stream; cleared on disconnect.
/// </summary>
public class MerchantUserMappingService : IMerchantUserMappingService
{
    private readonly ConcurrentDictionary<string, string> _map = new();

    public void Register(string merchantId, string userId) =>
        _map[merchantId] = userId;

    public void Unregister(string merchantId) =>
        _map.TryRemove(merchantId, out _);

    public bool TryGetUserId(string merchantId, out string? userId) =>
        _map.TryGetValue(merchantId, out userId);
}
