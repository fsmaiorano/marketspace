namespace BuildingBlocks.Message.Abstractions;

public interface IEventSerializer
{
    string Serialize(object value);
    T Deserialize<T>(string payload);
}
