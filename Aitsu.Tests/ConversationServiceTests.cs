using Aitsu;

namespace Aitsu.Tests;

public sealed class ConversationServiceTests
{
    [Fact]
    public async Task SendAsync_sends_previous_history_and_saves_successful_turn()
    {
        var client = new FakeChatClient();
        var conversation = new ConversationService(client);

        var firstResponse = await conversation.SendAsync("最初の質問");
        var secondResponse = await conversation.SendAsync("続きの質問");

        Assert.Equal("返答1", firstResponse);
        Assert.Equal("返答2", secondResponse);
        Assert.Equal(2, client.ReceivedHistories.Count);
        Assert.Empty(client.ReceivedHistories[0]);
        Assert.Collection(
            client.ReceivedHistories[1],
            message =>
            {
                Assert.Equal("user", message.Role);
                Assert.Equal("最初の質問", message.Content);
            },
            message =>
            {
                Assert.Equal("assistant", message.Role);
                Assert.Equal("返答1", message.Content);
            });
    }

    [Fact]
    public async Task Clear_removes_saved_history()
    {
        var client = new FakeChatClient();
        var conversation = new ConversationService(client);

        await conversation.SendAsync("削除前の質問");
        conversation.Clear();
        await conversation.SendAsync("削除後の質問");

        Assert.Empty(client.ReceivedHistories[1]);
    }

    private sealed class FakeChatClient : IChatClient
    {
        private int _responseNumber;

        public List<IReadOnlyList<ConversationMessage>> ReceivedHistories { get; } =
            [];

        public Task<string> GenerateResponseAsync(
            string userMessage,
            IReadOnlyList<ConversationMessage> history,
            Action<string>? onToken = null,
            CancellationToken cancellationToken = default)
        {
            ReceivedHistories.Add(history.ToArray());
            _responseNumber++;
            var response = $"返答{_responseNumber}";
            onToken?.Invoke(response);
            return Task.FromResult(response);
        }
    }
}
