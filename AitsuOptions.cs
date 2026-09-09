using System.Text;

namespace Aitsu;

public sealed class AitsuOptions
{
    public const string DefaultModel = "llama3.2";
    public const string PersonaFileName = "persona.txt";
    public const string DefaultPersona =
        "あなたはaitsuです。日本語で簡潔に応答してください。";
    public const int MaximumInputCharacters = 4_000;
    public const int MaximumResponseCharacters = 8_000;
    public const int MaximumHistoryMessages = 20;
    public const int MaximumHistoryCharacters = 16_000;
    public const int MaximumResponseBodyCharacters = 4_000_000;
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(5);

    public string Model { get; init; } = DefaultModel;

    public Uri Endpoint { get; init; } = CreateDefaultEndpoint();

    public string Persona { get; init; } = DefaultPersona;

    public static AitsuOptions Load()
    {
        var model = Environment.GetEnvironmentVariable("OLLAMA_MODEL");
        var endpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT");
        var personaFile = Environment.GetEnvironmentVariable(
            "AITSU_PERSONA_FILE");

        return new AitsuOptions
        {
            Model = string.IsNullOrWhiteSpace(model)
                ? DefaultModel
                : model,
            Endpoint = CreateEndpoint(endpoint),
            Persona = LoadPersona(personaFile)
        };
    }

    private static Uri CreateDefaultEndpoint()
    {
        return new Uri("http://localhost:11434/api/chat");
    }

    private static Uri CreateEndpoint(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return CreateDefaultEndpoint();
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp
                && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new InvalidOperationException(
                "環境変数 OLLAMA_ENDPOINT のURLが正しくありません。");
        }

        if (uri.Scheme == Uri.UriSchemeHttp && !uri.IsLoopback)
        {
            throw new InvalidOperationException(
                "外部のHTTP接続は許可していません。HTTPSを使用してください。");
        }

        return uri;
    }

    private static string LoadPersona(string? personaFile)
    {
        var isExplicitPath = !string.IsNullOrWhiteSpace(personaFile);
        var path = isExplicitPath
            ? ResolvePersonaPath(personaFile!)
            : Path.Combine(AppContext.BaseDirectory, PersonaFileName);

        if (!File.Exists(path))
        {
            if (isExplicitPath)
            {
                throw new FileNotFoundException(
                    "AITSU_PERSONA_FILEで指定されたファイルが見つかりません。",
                    path);
            }

            return DefaultPersona;
        }

        var persona = File.ReadAllText(path, Encoding.UTF8).Trim();
        if (string.IsNullOrWhiteSpace(persona))
        {
            return DefaultPersona;
        }

        if (persona.Length > MaximumInputCharacters)
        {
            throw new InvalidOperationException(
                $"人格設定は{MaximumInputCharacters}文字以内で指定してください。");
        }

        return persona;
    }

    private static string ResolvePersonaPath(string personaFile)
    {
        if (Path.IsPathRooted(personaFile))
        {
            return personaFile;
        }

        var currentDirectoryPath = Path.GetFullPath(
            personaFile,
            Directory.GetCurrentDirectory());
        if (File.Exists(currentDirectoryPath))
        {
            return currentDirectoryPath;
        }

        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, personaFile));
    }
}
