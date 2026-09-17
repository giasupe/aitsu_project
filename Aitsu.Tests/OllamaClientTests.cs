using System.Net;
using System.Text;
using System.Text.Json;
using Aitsu;

namespace Aitsu.Tests;

public sealed class OllamaClientTests
{
    [Fact]
    public async Task GenerateResponseAsync_limits_streamed_output_to_100_characters()
    {
        var responseText = new string('あ', 150);
        var responseBody = JsonSerializer.Serialize(
                new
                {
                    message = new
                    {
                        content = responseText
                    },
                    done = true
                })
            + "\n";
        var handler = new StubHttpMessageHandler(responseBody);
        using var httpClient = new HttpClient(handler);
        var options = new AitsuOptions
        {
            Model = "test-model",
            Persona = "テスト用人格",
            Endpoint = new Uri("http://localhost:11434/api/chat")
        };
        var client = new OllamaClient(httpClient, options);
        var streamedOutput = new StringBuilder();

        var result = await client.GenerateResponseAsync(
            "テスト入力",
            Array.Empty<ConversationMessage>(),
            text =>
            {
                streamedOutput.Append(text);
            });

        Assert.Equal(100, result.Length);
        Assert.Equal(result, streamedOutput.ToString());
        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request!.Method);

        Assert.NotNull(handler.RequestBody);
        Assert.Contains("\"stream\":true", handler.RequestBody);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseBody;

        public StubHttpMessageHandler(string responseBody)
        {
            _responseBody = responseBody;
        }

        public HttpRequestMessage? Request { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = await request.Content!.ReadAsStringAsync(
                cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    _responseBody,
                    Encoding.UTF8,
                    "application/x-ndjson")
            };
        }
    }
}
