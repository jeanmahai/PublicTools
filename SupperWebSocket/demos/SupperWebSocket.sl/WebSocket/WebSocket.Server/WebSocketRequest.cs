using System;

namespace WebSocket.WebSocket.Server
{
    [Serializable]
    public class WebSocketRequest
    {
        public string Handler { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public string ClientCallbackID { get; set; }
    }
}
