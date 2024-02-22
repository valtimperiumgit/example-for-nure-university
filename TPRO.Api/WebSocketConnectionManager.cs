using System.Net.WebSockets;

namespace TPRO.Api;

public class WebsocketConnectionManager
{
    private readonly List<WebSocket> _sockets = new List<WebSocket>();

    public void AddSocket(WebSocket socket)
    {
        _sockets.Add(socket);
    }

    public async Task BroadcastAsync(byte[] buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        foreach (var socket in _sockets.ToList())
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), messageType, endOfMessage, cancellationToken);
            }
        }
    }

    public void RemoveSocket(WebSocket socket)
    {
        _sockets.Remove(socket);
    }

    public List<WebSocket> GetAllSockets()
    {
        return _sockets;
    }
}