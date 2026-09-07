namespace Aitsu;

public sealed record ConversationMessage(string Role, string Content);

public sealed class ConversationService
{
    private const int MaximumHistoryMessages = 20;
    private readonly OllamaClient _client;
    private readonly List<ConversationMessage> _history = [];

    public ConversationService(OllamaClient client)
    {
        _client = client;
    }

    public async Task<string> SendAsync(
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GenerateResponseAsync(
            userMessage,
            _history,
            cancellationToken);

        _history.Add(new ConversationMessage("user", userMessage));
        _history.Add(new ConversationMessage("assistant", response));
        TrimHistory();

        return response;
    }

    public void Clear()
    {
        _history.Clear();
    }

    private void TrimHistory()
    {
        if (_history.Count <= MaximumHistoryMessages)
        {
            return;
        }

        _history.RemoveRange(
            0,
            _history.Count - MaximumHistoryMessages);
    }
}
