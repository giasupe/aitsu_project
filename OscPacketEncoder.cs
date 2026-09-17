using System.Text;

namespace Aitsu;

internal static class OscPacketEncoder
{
    public static byte[] EncodeChatboxInput(
        string message,
        bool sendImmediately,
        bool notification)
    {
        using var packet = new MemoryStream();
        WriteOscString(packet, "/chatbox/input");
        WriteOscString(
            packet,
            $",s{ToBooleanTypeTag(sendImmediately)}{ToBooleanTypeTag(notification)}");
        WriteOscString(packet, message);
        return packet.ToArray();
    }

    public static byte[] EncodeTyping(bool isTyping)
    {
        using var packet = new MemoryStream();
        WriteOscString(packet, "/chatbox/typing");
        WriteOscString(packet, $",{ToBooleanTypeTag(isTyping)}");
        return packet.ToArray();
    }

    private static char ToBooleanTypeTag(bool value)
    {
        return value ? 'T' : 'F';
    }

    private static void WriteOscString(
        Stream stream,
        string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var paddedLength = (bytes.Length + 1 + 3) / 4 * 4;

        stream.Write(bytes);
        stream.WriteByte(0);

        for (var index = bytes.Length + 1; index < paddedLength; index++)
        {
            stream.WriteByte(0);
        }
    }
}
