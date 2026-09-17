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
        Action<string>? onToken = null,
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
            stream = true
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

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await ReadResponseBodyAsync(
                    response.Content,
                    requestTimeout.Token);
                var detail = errorBody.Length > 2_000
                    ? errorBody[..2_000]
                    : errorBody;

                throw new InvalidOperationException(
                    $"Ollamaエラー ({(int)response.StatusCode}): {detail}");
            }

            var text = await ReadStreamingResponseAsync(
                response.Content,
                onToken,
                requestTimeout.Token);
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

    private static async Task<string> ReadStreamingResponseAsync(
        HttpContent content,
        Action<string>? onToken,
        CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(
            cancellationToken);
        using var reader = new StreamReader(stream);
        var response = new StringBuilder();
        var responseBodyCharacters = 0;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            responseBodyCharacters += line.Length;
            if (responseBodyCharacters
                > AitsuOptions.MaximumResponseBodyCharacters)
            {
                throw new InvalidOperationException(
                    $"Ollamaの応答が{AitsuOptions.MaximumResponseBodyCharacters}文字を超えています。");
            }

            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;

            if (root.TryGetProperty("error", out var error))
            {
                throw new InvalidOperationException(
                    $"Ollamaエラー: {error}");
            }

            if (!root.TryGetProperty("message", out var message)
                || !message.TryGetProperty("content", out var contentPart)
                || contentPart.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var chunk = contentPart.GetString();
            if (string.IsNullOrEmpty(chunk))
            {
                continue;
            }

            var remainingCharacters =
                AitsuOptions.MaximumResponseCharacters - response.Length;
            if (remainingCharacters <= 0)
            {
                return response.ToString();
            }

            var limitedChunk = TakeCharacters(
                chunk,
                remainingCharacters);
            if (limitedChunk.Length == 0)
            {
                continue;
            }

            response.Append(limitedChunk);
            onToken?.Invoke(limitedChunk);

            if (response.Length
                >= AitsuOptions.MaximumResponseCharacters)
            {
                return response.ToString();
            }
        }

        return response.ToString();
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

        return TakeCharacters(
            response,
            AitsuOptions.MaximumResponseCharacters);
    }

    private static string TakeCharacters(
        string text,
        int maximumCharacters)
    {
        if (text.Length <= maximumCharacters)
        {
            return text;
        }

        var length = maximumCharacters;
        if (length > 0 && char.IsHighSurrogate(text[length - 1]))
        {
            length--;
        }

        return text[..length];
    }
}
