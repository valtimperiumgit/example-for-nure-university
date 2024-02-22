using System.Net.WebSockets;
using TPRO.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton<WebsocketConnectionManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocketConnectionManager = app.Services.GetRequiredService<WebsocketConnectionManager>();
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        webSocketConnectionManager.AddSocket(webSocket);
        
        await Echo(context, webSocket, webSocketConnectionManager);
        webSocketConnectionManager.RemoveSocket(webSocket);
    }
    else
    {
        await next();
    }
});

async Task Echo(HttpContext context, WebSocket webSocket, WebsocketConnectionManager manager)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result;
    do
    {
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        
        if (result.MessageType == WebSocketMessageType.Binary && !result.CloseStatus.HasValue)
        {
            await manager.BroadcastAsync(buffer, result.MessageType, result.EndOfMessage, CancellationToken.None);
        }

    } while (!result.CloseStatus.HasValue);

    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();