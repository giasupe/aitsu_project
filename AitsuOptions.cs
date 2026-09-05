namespace Aitsu;

public sealed class AitsuOptions
{
    public required string ApiKey { get; init; }

    public string Model { get; init; } = "gpt-4.1-mini";

    public Uri Endpoint { get; init; } = new("https://api.openai.com/v1/responses");

    public string Instructions { get; init; } = string.Empty;

    public static AitsuOptions Load()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "環境変数 OPENAI_API_KEY が設定されていません。");
        }

        var model = Environment.GetEnvironmentVariable("OPENAI_MODEL");
        var personaPath = Environment.GetEnvironmentVariable("AITSU_PERSONA_FILE")
            ?? "persona.txt";

        if (!Path.IsPathRooted(personaPath))
        {
            personaPath = Path.Combine(AppContext.BaseDirectory, personaPath);
        }

        var instructions = File.Exists(personaPath)
            ? File.ReadAllText(personaPath)
            : DefaultInstructions;

        return new AitsuOptions
        {
            ApiKey = apiKey,
            Model = string.IsNullOrWhiteSpace(model) ? "gpt-4.1-mini" : model,
            Instructions = instructions
        };
    }

    private const string DefaultInstructions = """
        あなたはaitsuシステムの対話AIです。
        日本語で簡潔に応答してください。
        """;
}
