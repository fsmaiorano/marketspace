using System.Text.Json;
using BuildingBlocks.Message.Abstractions;

namespace BuildingBlocks.Message.Serialization;

public sealed class SystemTextJsonEventSerializer : IEventSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public string Serialize(object value) => JsonSerializer.Serialize(value, DefaultOptions);

    public T Deserialize<T>(string payload) => JsonSerializer.Deserialize<T>(payload, DefaultOptions)
                                         ?? throw new InvalidOperationException("Unable to deserialize event payload.");
}
