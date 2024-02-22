using System.Net.WebSockets;

namespace TPRO.Api.WebSokets;

public class WebSocketMiddleware(RequestDelegate next)
{
    private const int MaxBufferSize = 1024 * 4;

    public async Task InvokeAsync(HttpContext context, WebSocketConnectionManager webSocketConnectionManager)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            webSocketConnectionManager.AddSocket(webSocket);

            await Handle(context, webSocket, webSocketConnectionManager);
            webSocketConnectionManager.RemoveSocket(webSocket);
        }
        else
        {
            await next(context);
        }
    }

    private static async Task Handle(HttpContext context, WebSocket webSocket, WebSocketConnectionManager manager)
    {
        var buffer = new byte[MaxBufferSize];
        
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result is { MessageType: WebSocketMessageType.Binary, CloseStatus: null })
            {
                await manager.BroadcastAsync(buffer, result.MessageType, result.EndOfMessage, CancellationToken.None);
            }

        } 
        while (!result.CloseStatus.HasValue);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
}