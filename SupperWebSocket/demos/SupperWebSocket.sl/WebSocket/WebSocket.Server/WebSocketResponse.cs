using System;

namespace WebSocket.WebSocket.Server
{
    [Serializable]
    public class WebSocketResponse
    {
        public string Handler { get; set; }
        public string Data { get; set; }
    }
}
