using System.Text;

namespace Aitsu;

internal static class Program
{
    public static async Task Main()
    {
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
                Timeout = TimeSpan.FromMinutes(5)
            };
            var client = new OllamaClient(httpClient, options);
            var conversation = new ConversationService(client);

            Console.WriteLine("aitsuを開始しました。");
            Console.WriteLine("終了: /exit  履歴削除: /clear");

            while (!cancellation.IsCancellationRequested)
            {
                Console.Write("You> ");
                var input = Console.ReadLine();

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

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                try
                {
                    var response = await conversation.SendAsync(
                        input,
                        cancellation.Token);
                    Console.WriteLine($"aitsu> {response}");
                }
                catch (OperationCanceledException)
                    when (cancellation.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine(
                        $"エラー: {exception.Message}");
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
}
