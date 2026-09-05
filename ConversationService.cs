namespace Aitsu;

public sealed class ConversationService
{
    private const int MaximumHistoryTurns = 20;
    private readonly OpenAIClient _client;
    private readonly List<ConversationTurn> _history = [];

    public ConversationService(OpenAIClient client)
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

        _history.Add(new ConversationTurn("user", userMessage));
        _history.Add(new ConversationTurn("assistant", response));

        if (_history.Count > MaximumHistoryTurns)
        {
            _history.RemoveRange(
                0,
                _history.Count - MaximumHistoryTurns);
        }

        return response;
    }

    public void Clear()
    {
        _history.Clear();
    }
}
