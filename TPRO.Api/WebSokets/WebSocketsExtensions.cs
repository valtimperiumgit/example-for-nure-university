namespace TPRO.Api.WebSokets;

public static class WebSocketsExtensions
{
    public static IApplicationBuilder UseWebSocketsMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<WebSocketMiddleware>();
    }
}