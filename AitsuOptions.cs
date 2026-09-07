namespace Aitsu;

public sealed class AitsuOptions
{
    public string Model { get; init; } = "llama3.2";

    public Uri Endpoint { get; init; } =
        new("http://localhost:11434/api/chat");

    public string Persona { get; init; } =
        "あなたはaitsuです。日本語で簡潔に応答してください。";

    public static AitsuOptions Load()
    {
        var model = Environment.GetEnvironmentVariable("OLLAMA_MODEL");
        var endpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT");
        var personaFile = Environment.GetEnvironmentVariable(
            "AITSU_PERSONA_FILE");

        return new AitsuOptions
        {
            Model = string.IsNullOrWhiteSpace(model)
                ? "llama3.2"
                : model,
            Endpoint = CreateEndpoint(endpoint),
            Persona = LoadPersona(personaFile)
        };
    }

    private static Uri CreateEndpoint(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return new Uri("http://localhost:11434/api/chat");
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException(
                "環境変数 OLLAMA_ENDPOINT のURLが正しくありません。");
        }

        return uri;
    }

    private static string LoadPersona(string? personaFile)
    {
        var path = string.IsNullOrWhiteSpace(personaFile)
            ? Path.Combine(AppContext.BaseDirectory, "persona.txt")
            : personaFile;

        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, path);
        }

        if (!File.Exists(path))
        {
            return "あなたはaitsuです。日本語で簡潔に応答してください。";
        }

        var persona = File.ReadAllText(path).Trim();
        return string.IsNullOrWhiteSpace(persona)
            ? "あなたはaitsuです。日本語で簡潔に応答してください。"
            : persona;
    }
}
