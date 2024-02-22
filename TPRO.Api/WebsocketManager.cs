using System.Net.WebSockets;

namespace TPRO.Api;

public static class WebsocketManager
{
    public static readonly List<WebSocket> Sockets = new List<WebSocket>();
}