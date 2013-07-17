using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSocket.WebSocket.Server;

namespace Server
{
    public class Login:IWebSocketHandler
    {
        public WebSocketResponse Analyze(WebSocketRequest request, WebSocketResponse response)
        {
            response.Data = "test login";
            return response;
        }
    }
}