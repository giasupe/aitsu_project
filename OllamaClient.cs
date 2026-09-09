using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Aitsu;

public sealed class OllamaClient : IChatClient
{
    private readonly HttpClient _httpClient;
    private readonly AitsuOptions _options;

    public OllamaClient(HttpClient httpClient, AitsuOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<string> GenerateResponseAsync(
        string userMessage,
        IReadOnlyList<ConversationMessage> history,
        CancellationToken cancellationToken = default)
    {
        var messages = new[]
            {
                new
                {
                    role = "system",
                    content = _options.Persona
                }
            }
            .Concat(history.Select(message => new
            {
                role = message.Role,
                content = message.Content
            }))
            .Append(new
            {
                role = "user",
                content = userMessage
            })
            .ToArray();

        var requestBody = new
        {
            model = _options.Model,
            messages,
            stream = false
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            _options.Endpoint)
        {
            Content = JsonContent.Create(requestBody)
        };

        using var requestTimeout = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);
        requestTimeout.CancelAfter(AitsuOptions.RequestTimeout);

        try
        {
            using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    requestTimeout.Token);
            var responseBody = await ReadResponseBodyAsync(
                response.Content,
                requestTimeout.Token);

            if (!response.IsSuccessStatusCode)
            {
                var detail = responseBody.Length > 2_000
                    ? responseBody[..2_000]
                    : responseBody;

                throw new InvalidOperationException(
                    $"Ollamaエラー ({(int)response.StatusCode}): {detail}");
            }

            using var document = JsonDocument.Parse(responseBody);
            if (!document.RootElement.TryGetProperty(
                    "message",
                    out var message)
                || !message.TryGetProperty("content", out var content)
                || content.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    "Ollamaの応答からテキストを取得できませんでした。");
            }

            var text = content.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException(
                    "Ollamaの応答が空でした。");
            }

            return LimitResponseLength(text.Trim());
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested
                && requestTimeout.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Ollamaへのリクエストが{AitsuOptions.RequestTimeout.TotalMinutes:0}分でタイムアウトしました。",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new InvalidOperationException(
                "Ollamaに接続できません。Ollamaが起動しているか確認してください。",
                exception);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Ollamaの応答をJSONとして解析できませんでした。",
                exception);
        }
    }

    private static async Task<string> ReadResponseBodyAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(
            cancellationToken);
        using var reader = new StreamReader(stream);
        var builder = new StringBuilder();
        var buffer = new char[8_192];

        while (true)
        {
            var charactersRead = await reader.ReadAsync(
                buffer.AsMemory(),
                cancellationToken);
            if (charactersRead == 0)
            {
                return builder.ToString();
            }

            if (builder.Length + charactersRead
                > AitsuOptions.MaximumResponseBodyCharacters)
            {
                throw new InvalidOperationException(
                    $"Ollamaの応答が{AitsuOptions.MaximumResponseBodyCharacters}文字を超えています。");
            }

            builder.Append(buffer, 0, charactersRead);
        }
    }

    private static string LimitResponseLength(string response)
    {
        if (response.Length <= AitsuOptions.MaximumResponseCharacters)
        {
            return response;
        }

        const string truncationNotice = "\n[応答が長いため省略しました]";
        var contentLength = AitsuOptions.MaximumResponseCharacters
            - truncationNotice.Length;
        return response[..contentLength].TrimEnd() + truncationNotice;
    }
}
