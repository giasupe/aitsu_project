namespace Aitsu;

public interface IChatClient
{
    Task<string> GenerateResponseAsync(
        string userMessage,
        IReadOnlyList<ConversationMessage> history,
        Action<string>? onToken = null,
        CancellationToken cancellationToken = default);
}
