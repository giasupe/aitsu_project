namespace Aitsu;

public interface IChatClient
{
    Task<string> GenerateResponseAsync(
        string userMessage,
        IReadOnlyList<ConversationMessage> history,
        CancellationToken cancellationToken = default);
}
