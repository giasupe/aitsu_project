using System.Text;

namespace Aitsu;

internal static class ChatboxTextFormatter
{
    public static IReadOnlyList<string> Split(string message)
    {
        var normalized = message
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return [];
        }

        var chunks = new List<string>();
        var current = new StringBuilder();
        var lineCount = 1;

        foreach (var rune in normalized.EnumerateRunes())
        {
            var addsLine = rune.Value == '\n';
            var exceedsCharacters =
                current.Length + rune.Utf16SequenceLength
                > AitsuOptions.ChatboxMaximumCharacters;
            var exceedsLines =
                lineCount + (addsLine ? 1 : 0)
                > AitsuOptions.ChatboxMaximumLines;

            if (current.Length > 0
                && (exceedsCharacters || exceedsLines))
            {
                chunks.Add(current.ToString());
                current.Clear();
                lineCount = 1;
            }

            current.Append(rune.ToString());
            if (addsLine)
            {
                lineCount++;
            }
        }

        if (current.Length > 0)
        {
            chunks.Add(current.ToString());
        }

        return chunks;
    }
}
