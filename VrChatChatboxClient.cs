using System.Net;
using System.Net.Sockets;

namespace Aitsu;

public sealed class VrChatChatboxClient : IChatboxClient
{
    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _endpoint;
    private readonly bool _notification;

    public VrChatChatboxClient(AitsuOptions options)
    {
        _udpClient = new UdpClient(options.ChatboxAddress.AddressFamily);
        _endpoint = new IPEndPoint(
            options.ChatboxAddress,
            options.ChatboxPort);
        _notification = options.ChatboxNotification;
    }

    public async Task SendMessageAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        foreach (var chunk in ChatboxTextFormatter.Split(message))
        {
            var packet = OscPacketEncoder.EncodeChatboxInput(
                chunk,
                sendImmediately: true,
                notification: _notification);
            await SendPacketAsync(packet, cancellationToken);
        }
    }

    public Task SetTypingAsync(
        bool isTyping,
        CancellationToken cancellationToken = default)
    {
        var packet = OscPacketEncoder.EncodeTyping(isTyping);
        return SendPacketAsync(packet, cancellationToken);
    }

    public void Dispose()
    {
        _udpClient.Dispose();
    }

    private async Task SendPacketAsync(
        byte[] packet,
        CancellationToken cancellationToken)
    {
        try
        {
            await _udpClient.SendAsync(
                packet,
                _endpoint,
                cancellationToken);
        }
        catch (SocketException exception)
        {
            throw new InvalidOperationException(
                "VRChat ChatboxへのOSC送信に失敗しました。",
                exception);
        }
    }
}
