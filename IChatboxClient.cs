namespace Aitsu;

public interface IChatboxClient : IDisposable
{
    Task SendMessageAsync(
        string message,
        CancellationToken cancellationToken = default);

    Task SetTypingAsync(
        bool isTyping,
        CancellationToken cancellationToken = default);
}

public sealed class NullChatboxClient : IChatboxClient
{
    public Task SendMessageAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SetTypingAsync(
        bool isTyping,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}
