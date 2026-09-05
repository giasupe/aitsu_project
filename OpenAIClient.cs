using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Aitsu;

public sealed record ConversationTurn(string Role, string Content);

public sealed class OpenAIClient
{
    private readonly HttpClient _httpClient;
    private readonly AitsuOptions _options;

    public OpenAIClient(HttpClient httpClient, AitsuOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<string> GenerateResponseAsync(
        string userMessage,
        IReadOnlyList<ConversationTurn> history,
        CancellationToken cancellationToken = default)
    {
        var input = history
            .Select(turn => new
            {
                role = turn.Role,
                content = turn.Content
            })
            .Append(new
            {
                role = "user",
                content = userMessage
            })
            .ToArray();

        var requestBody = new
        {
            model = _options.Model,
            instructions = _options.Instructions,
            input,
            store = false
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = responseBody.Length > 2_000
                ? responseBody[..2_000]
                : responseBody;

            throw new InvalidOperationException(
                $"OpenAI APIエラー ({(int)response.StatusCode}): {detail}");
        }

        using var document = JsonDocument.Parse(responseBody);
        var text = ExtractOutputText(document.RootElement);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException(
                "OpenAI APIの応答からテキストを取得できませんでした。");
        }

        return text.Trim();
    }

    private static string? ExtractOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputText)
            && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString();
        }

        if (!root.TryGetProperty("output", out var output)
            || output.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var texts = output.EnumerateArray()
            .Where(item =>
                item.TryGetProperty("type", out var type)
                && type.GetString() == "message")
            .SelectMany(item =>
                item.TryGetProperty("content", out var content)
                    && content.ValueKind == JsonValueKind.Array
                    ? content.EnumerateArray()
                    : Enumerable.Empty<JsonElement>())
            .Where(item =>
                item.TryGetProperty("type", out var type)
                && type.GetString() == "output_text"
                && item.TryGetProperty("text", out var text)
                && text.ValueKind == JsonValueKind.String)
            .Select(item => item.GetProperty("text").GetString())
            .Where(text => !string.IsNullOrWhiteSpace(text));

        return string.Join(Environment.NewLine, texts);
    }
}
