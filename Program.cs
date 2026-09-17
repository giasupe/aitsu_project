using System.Text;

namespace Aitsu;

internal static class Program
{
    public static async Task Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        try
        {
            var options = AitsuOptions.Load();
            using var httpClient = new HttpClient
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan
            };
            var client = new OllamaClient(httpClient, options);
            var conversation = new ConversationService(client);
            using var chatbox = new VrChatChatboxClient(options);
            var chatboxEnabled = options.ChatboxEnabled;

            Console.WriteLine("aitsuを開始しました。");
            Console.WriteLine("終了: /exit  履歴削除: /clear");
            Console.WriteLine(
                $"VRChat Chatbox: {(chatboxEnabled ? "有効" : "無効")}");
            Console.WriteLine(
                "切り替え: /chatbox on  /chatbox off  /chatbox status");

            while (!cancellation.IsCancellationRequested)
            {
                Console.Write("You> ");
                string? input;
                try
                {
                    input = await Console.In.ReadLineAsync(
                        cancellation.Token);
                }
                catch (OperationCanceledException)
                    when (cancellation.IsCancellationRequested)
                {
                    break;
                }

                if (input is null ||
                    input.Equals(
                        "/exit",
                        StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (input.Equals(
                        "/clear",
                        StringComparison.OrdinalIgnoreCase))
                {
                    conversation.Clear();
                    Console.WriteLine("会話履歴を削除しました。");
                    continue;
                }

                if (TryHandleChatboxCommand(input, ref chatboxEnabled))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                var typingShown = false;
                try
                {
                    if (chatboxEnabled)
                    {
                        await chatbox.SetTypingAsync(
                            true,
                            cancellation.Token);
                        typingShown = true;
                    }

                    Console.Write("aitsu> ");
                    var response = await conversation.SendAsync(
                        input,
                        token => Console.Write(token),
                        cancellation.Token);
                    Console.WriteLine();

                    if (chatboxEnabled)
                    {
                        var chatboxMessage =
                            $"You> {input}\naitsu> {response}";
                        await chatbox.SendMessageAsync(
                            chatboxMessage,
                            cancellation.Token);
                    }
                }
                catch (OperationCanceledException)
                    when (cancellation.IsCancellationRequested)
                {
                    Console.WriteLine();
                    break;
                }
                catch (Exception exception)
                {
                    Console.WriteLine();
                    Console.Error.WriteLine(
                        $"エラー: {exception.Message}");
                }
                finally
                {
                    if (typingShown)
                    {
                        try
                        {
                            await chatbox.SetTypingAsync(
                                false,
                                CancellationToken.None);
                        }
                        catch (Exception exception)
                        {
                            Console.Error.WriteLine(
                                $"Chatboxの入力中表示を解除できませんでした: {exception.Message}");
                        }
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"起動エラー: {exception.Message}");
            Environment.ExitCode = 1;
        }
    }

    private static bool TryHandleChatboxCommand(
        string input,
        ref bool chatboxEnabled)
    {
        var parts = input.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries);
        if (parts.Length == 0
            || !parts[0].Equals(
                "/chatbox",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (parts.Length == 1)
        {
            chatboxEnabled = !chatboxEnabled;
            PrintChatboxStatus(chatboxEnabled);
            return true;
        }

        if (parts.Length != 2)
        {
            PrintChatboxUsage();
            return true;
        }

        switch (parts[1].ToLowerInvariant())
        {
            case "on":
                chatboxEnabled = true;
                PrintChatboxStatus(chatboxEnabled);
                break;
            case "off":
                chatboxEnabled = false;
                PrintChatboxStatus(chatboxEnabled);
                break;
            case "status":
                PrintChatboxStatus(chatboxEnabled);
                break;
            default:
                PrintChatboxUsage();
                break;
        }

        return true;
    }

    private static void PrintChatboxStatus(bool chatboxEnabled)
    {
        Console.WriteLine(
            $"VRChat Chatbox: {(chatboxEnabled ? "有効" : "無効")}");
    }

    private static void PrintChatboxUsage()
    {
        Console.WriteLine(
            "使い方: /chatbox on | /chatbox off | /chatbox status");
    }
}
