namespace Aitsu;

public sealed record ConversationMessage(string Role, string Content);

public sealed class ConversationService
{
    private readonly IChatClient _client;
    private readonly List<ConversationMessage> _history = [];

    public ConversationService(IChatClient client)
    {
        _client = client;
    }

    public async Task<string> SendAsync(
        string userMessage,
        Action<string>? onToken = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException(
                "入力が空です。",
                nameof(userMessage));
        }

        if (userMessage.Length > AitsuOptions.MaximumInputCharacters)
        {
            throw new ArgumentException(
                $"入力は{AitsuOptions.MaximumInputCharacters}文字以内で指定してください。",
                nameof(userMessage));
        }

        var response = await _client.GenerateResponseAsync(
            userMessage,
            _history,
            onToken,
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
        while (_history.Count > AitsuOptions.MaximumHistoryMessages
            || GetHistoryCharacters() > AitsuOptions.MaximumHistoryCharacters)
        {
            var messagesToRemove = Math.Min(2, _history.Count);
            _history.RemoveRange(0, messagesToRemove);
        }
    }

    private int GetHistoryCharacters()
    {
        return _history.Sum(message => message.Content.Length);
    }
}
