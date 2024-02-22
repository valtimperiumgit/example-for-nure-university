using System.Net.WebSockets;
namespace TPRO.Api.WebSokets;

public class WebSocketConnectionManager
{
    private readonly List<WebSocket> _sockets = [];
    
    private const int OffSet = 0;

    public void AddSocket(WebSocket socket)
    {
        _sockets.Add(socket);
    }
    
    public void RemoveSocket(WebSocket socket)
    {
        _sockets.Remove(socket);
    }

    public async Task BroadcastAsync(byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        foreach (var socket in _sockets.ToList().Where(socket => socket.State == WebSocketState.Open))
        {
            await socket.SendAsync(new ArraySegment<byte>(buffer, OffSet, buffer.Length), messageType, endOfMessage, cancellationToken);
        }
    }
}