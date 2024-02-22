using System.Net.WebSockets;

namespace TPRO.Core.WebSockets;

public static class WebSocketsUtils
{
    private static readonly Uri Address = new("ws://localhost:5184/");

    public static async Task SendImageViaWebSocket(byte[] imageBytes)
    {
        using var client = new ClientWebSocket();
        await client.ConnectAsync(Address, CancellationToken.None);
        await client.SendAsync(new ArraySegment<byte>(imageBytes), WebSocketMessageType.Binary, true, CancellationToken.None);
        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }
}