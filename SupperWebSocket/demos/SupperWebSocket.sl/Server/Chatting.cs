using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSocket.WebSocket.Server;

namespace Server
{
    public class Chatting : IWebSocketHandler
    {
        public WebSocketResponse Analyze(WebSocketRequest request, WebSocketResponse response)
        {
            response.Data = string.Format("{0} {1}", DateTime.Now, request.Body);
            return response;
        }
    }
}