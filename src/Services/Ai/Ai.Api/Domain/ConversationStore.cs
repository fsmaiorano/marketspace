namespace Ai.Api.Domain;

public record ConversationMessage(string Role, string Content);

public class ConversationStore
{
    private readonly Dictionary<string, List<ConversationMessage>> _sessions = new();
    private const int MaxHistoryLength = 10;

    public IReadOnlyList<ConversationMessage> GetHistory(string sessionId) =>
        _sessions.TryGetValue(sessionId, out List<ConversationMessage>? history)
            ? history.AsReadOnly()
            : [];

    public void AddMessage(string sessionId, string role, string content)
    {
        if (!_sessions.TryGetValue(sessionId, out List<ConversationMessage>? history))
        {
            history = [];
            _sessions[sessionId] = history;
        }

        history.Add(new ConversationMessage(role, content));

        if (history.Count > MaxHistoryLength * 2)
            history.RemoveRange(0, 2);
    }

    public void Clear(string sessionId) => _sessions.Remove(sessionId);
}
