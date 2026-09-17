namespace Aitsu.Tests;

public sealed class ChatboxTextFormatterTests
{
    [Fact]
    public void Split_keeps_each_chunk_within_chatbox_limits()
    {
        var message = new string('あ', 300)
            + "\n"
            + new string('い', 300);

        var chunks = ChatboxTextFormatter.Split(message);

        Assert.NotEmpty(chunks);
        Assert.All(
            chunks,
            chunk =>
            {
                Assert.True(
                    chunk.Length <= AitsuOptions.ChatboxMaximumCharacters);
                Assert.True(
                    chunk.Count(character => character == '\n') + 1
                    <= AitsuOptions.ChatboxMaximumLines);
            });
    }

    [Fact]
    public void Split_returns_no_chunks_for_blank_message()
    {
        var chunks = ChatboxTextFormatter.Split(" \r\n ");

        Assert.Empty(chunks);
    }
}
