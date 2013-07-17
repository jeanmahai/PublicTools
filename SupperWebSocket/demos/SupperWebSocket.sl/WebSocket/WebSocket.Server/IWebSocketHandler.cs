namespace WebSocket.WebSocket.Server
{
    public interface IWebSocketHandler
    {
        WebSocketResponse Analyze(WebSocketRequest request,WebSocketResponse response);
    }
}