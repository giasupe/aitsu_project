using System.Net.Http.Json;
using System.Text.Json;

namespace Aitsu;

public sealed class OllamaClient
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

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new InvalidOperationException(
                "Ollamaに接続できません。Ollamaが起動しているか確認してください。",
                exception);
        }

        using (response)
        {
            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

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

            return text.Trim();
        }
    }
}
